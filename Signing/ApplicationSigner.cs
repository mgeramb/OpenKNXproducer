using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace OpenKNXproducer.Signing
{
    class ApplicationProgramHasher
    {
        public ApplicationProgramHasher(
                    FileInfo applProgFile,
                    IDictionary<string, string> mapBaggageIdToFileIntegrity,
                    string basePath,
                    int nsVersion,
                    bool patchIds = true)
        {
            //if ets6 use ApplicationProgramStoreHasher
            //with HashStore Method
            Assembly asm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.XmlSigning.dll"));

            string asmVersion = asm.GetName().Version.ToString();
            if(asm.GetName().Version.ToString().StartsWith("6.2.")) { //ab ETS6.2
                Assembly objm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.Common.dll"));
                System.Type scheme = objm.GetType("Knx.Ets.Common.Schema.KnxXmlSchemaVersion");
                object knxSchemaVersion = Enum.ToObject(scheme, nsVersion);
                _type = asm.GetType("Knx.Ets.XmlSigning.Signer.ApplicationProgramHasher");
                _instance = Activator.CreateInstance(_type, applProgFile, mapBaggageIdToFileIntegrity, patchIds, knxSchemaVersion);
            } else if(asm.GetName().Version.ToString().StartsWith("6.")) { //ab ETS6.0/6.1
                Assembly objm = Assembly.LoadFrom(Path.Combine(basePath, "Knx.Ets.Xml.ObjectModel.dll"));
                object knxSchemaVersion = Enum.ToObject(objm.GetType("Knx.Ets.Xml.ObjectModel.KnxXmlSchemaVersion"), nsVersion);
                _type = asm.GetType("Knx.Ets.XmlSigning.Signer.ApplicationProgramHasher");
                if (asmVersion.StartsWith("6.0"))
                    _type = asm.GetType("Knx.Ets.XmlSigning.ApplicationProgramHasher");
                _instance = Activator.CreateInstance(_type, applProgFile, mapBaggageIdToFileIntegrity, patchIds, knxSchemaVersion);
            } else { //für ETS5 und früher
                _type = asm.GetType("Knx.Ets.XmlSigning.ApplicationProgramHasher");
                _instance = Activator.CreateInstance(_type, applProgFile, mapBaggageIdToFileIntegrity, patchIds);
            }
        }

        public void Hash()
        {
             _type.GetMethod("HashFile", BindingFlags.Instance | BindingFlags.Public).Invoke(_instance, null);
        }

        public string OldApplProgId
        {
            get
            {
                return _type.GetProperty("OldApplProgId", BindingFlags.Public | BindingFlags.Instance).GetValue(_instance).ToString();
            }
        }

        public string NewApplProgId
        {
            get
            {
                return _type.GetProperty("NewApplProgId", BindingFlags.Public | BindingFlags.Instance).GetValue(_instance).ToString();
            }
        }

        public string GeneratedHashString
        {
            get
            {
                return _type.GetProperty("GeneratedHashString", BindingFlags.Public | BindingFlags.Instance).GetValue(_instance).ToString();
            }
        }

        private readonly object _instance;
        private readonly Type _type;
    }
}