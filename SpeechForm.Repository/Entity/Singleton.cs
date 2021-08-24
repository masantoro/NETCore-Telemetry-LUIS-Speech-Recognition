using SpeechForm.Repository.Entity.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechForm.Repository
{
    public sealed class Singleton
    {
        //private static Singleton instance = null;

        private static ConfigurationEntity configuration { get; set; }

        public Singleton()
        {

        }

        public static void ResetConfiguration()
        {
            configuration = null;
        }

        public static ConfigurationEntity Configuration
        {
            get
            {
                if (configuration == null)
                {
                    configuration = (new ConfigurationEntity()).Get();
                }
                return configuration;
            }
        }


        //public static Singleton Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //        {
        //            instance = new Singleton();
        //        }
        //        return instance;
        //    }
        //}
    }
}
