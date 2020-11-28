using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedModdingStation
{
    public class ErrorMessages
    {

        MainForm form;
        public ErrorMessages()
        {
            //NMSPackagesManager manager = new NMSPackagesManager(form);
            //this.form = form
        }

        public string ErrorMessage { get; set; }

        private readonly Dictionary<string, string> ErrorMessageList = new Dictionary<string, string>()
        {
            { "encountered error"," encountered an error while trying to read your Projects directory setting." },
            { "type", "Error type: " },
            { "config solution", "Solution: Please go to Config => Settings and setup your Paths before attempting to import a mod." },
            //{ 2, "Error!" },
            //{ 2, "Error type: " },
            //{ 2, "Error type: " },
            //{ 2, "Error type: " },
            //{ 2, "Error type: " },

        };

        public string ErrorMessageInBox()
        {
            ErrorMessage = form.applicationName + ErrorMessageList["encountered error"] + Environment.NewLine + Environment.NewLine;
            ErrorMessage += ErrorMessageList["type"] + MainForm.errorType.Misconfiguration + Environment.NewLine;
            ErrorMessage += ErrorMessageList["config solution"] + Environment.NewLine;
            return ErrorMessage;
        }


    }
}
