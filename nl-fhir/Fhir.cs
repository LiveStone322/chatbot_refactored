using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using System.Linq;

namespace nl_fhir
{
    public class Fhir
    {
        FhirClient client;

        const string endpoint = "localhost:4004";
        const string resPatient = "Patient";

        public Fhir()
        {
            client = new FhirClient(endpoint, new FhirClientSettings
            {
                Timeout = 0,
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                PreferredReturn = Prefer.ReturnMinimal
            });
        }

        public void AddPatient(string firstName, string lastName, bool isMale = true, string birthDate = "01-01-1990", string idVal = "000-00-0000")
        {
            var pat = new Patient();

            var id = new Identifier();
            id.System = "http://hl7.org/fhir/sid/us-ssn";
            id.Value = idVal;
            pat.Identifier.Add(id);

            var name = new HumanName().WithGiven(firstName).AndFamily(lastName);
            name.Prefix = new string[] { isMale ? "Mr." : "Mrs."};
            name.Use = HumanName.NameUse.Official;

            pat.Name.Add(name);

            pat.Gender = isMale ? AdministrativeGender.Male : AdministrativeGender.Female;

            pat.BirthDate = birthDate;

            pat.Deceased = new FhirBoolean(false);
            client.Create(pat);
        }

        public void AddObs(Patient p, string val = "")
        {
            client.Create<Observation>(new Observation()
            {
                Subject = p,
                Value = new FhirString(val)
            });
        }
        private Bundle SearchPatient(string firstName, string lastName)
        {
            var q = new Query()
                .For("Patient")
                .Where($"name:exact={firstName}")
                .Where($"family:exact={lastName}")
                .SummaryOnly().Include("Patient.managingOrganization")
                .LimitTo(20);
            return client.Search(q);
        }

        private System.Collections.Generic.List<Observation> GetPatientObservations(Patient p)
        {
            System.Collections.Generic.List<Observation> result = new System.Collections.Generic.List<Observation>();
            var q = new Query()
                 .For("Patient")
                 .Where($"subject:exact={p.ResourceIdentity}")
                 .SummaryOnly().Include("Patient.managingOrganization")
                 .LimitTo(20);
            var s = client.Search(q);

            while (s != null)
            {
                foreach(var r in s.GetResources())
                {
                    result.Add((Observation)r);
                }
                s = client.Continue(s);
            }
            return result;
        }
    }
}
