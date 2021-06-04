using System;
using System.Collections.Generic;
using System.Text;

namespace nl_fhir
{
    public class ActionsEnum
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
            ADD_USER,
            SHOW_INFORMATION,
            ADD_BIOMARK
        }
    }
}
