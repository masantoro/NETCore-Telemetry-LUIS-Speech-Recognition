using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechForm.Repository.FixedData
{
    public static class Speech
    {
        public static bool IsCrititalIntent(string CriticalIntent)
        {
            if (string.IsNullOrEmpty(CriticalIntent))
            {
                return false;
            } else
            {
                return GetAllCriticalIntent().Exists(x => x.ToUpper().Trim() == CriticalIntent.ToUpper().Trim());
            }
        }

        public static List<string> GetAllCriticalIntent()
        {
            return new List<string> {
                "legalProblems"
            };
        }


    }
}
