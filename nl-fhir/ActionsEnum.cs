using System;
using System.Collections.Generic;
using System.Text;

namespace nl_fhir
{
    public static class ActionsEnum
    {
        public enum Actions
        {
            //V1.0
            ReadMyBiomarkers,
            LoadFile,
            Answer,
            ConversationStart,
            SecretMessage,
            ConversationStartAnswer,
            GetPlot,
            ConnectToMobileApp,
            DoNothing,
            SystemAnswer,
            SendToApp,
            CallHuman,
            Chatting,
            AddBiomarks,
            PrintBiomarks,
            //V2.0
            UNSURE,
            ADD_USER,
            SHOW_INFORMATION,
            ADD_BIOMARK
        }

        public static Actions GetEnumFromText(string txt)
        {
            return dict.GetValueOrDefault(txt, Actions.UNSURE);
        }

        private static readonly Dictionary<string, Actions> dict = new Dictionary<string, Actions>()
        {
            { "add_user", Actions.ADD_USER },
            { "show_information", Actions.SHOW_INFORMATION },
            { "add_biomark", Actions.ADD_BIOMARK },
            { "load_file", Actions.LoadFile },
            { "show_plot", Actions.GetPlot },
            { "add_token", Actions.ConnectToMobileApp },
        };
    }
}
