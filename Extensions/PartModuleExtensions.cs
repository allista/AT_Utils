namespace AT_Utils
{
    public static class PartModuleExtensions
    {
        public static string Title(this PartModule pm) =>
            pm.part.partInfo != null ? pm.part.partInfo.title : pm.part.name;

        public static void EnableModule(this PartModule pm, bool enable) => pm.enabled = pm.isEnabled = enable;

        public static void ConfigurationInvalid(
            this PartModule pm,
            string msg,
            params object[] args
        )
        {
            msg = string.Format(msg, args);
            pm.Error(msg);
            Utils.Message(6, $"WARNING: {msg}.\nConfiguration of \"{pm.Title()}\" is INVALID.");
            pm.enabled = pm.isEnabled = false;
        }

        public static void Log(this PartModule pm, string msg) => Utils.Log($"{pm.GetID()}: {msg}");
        public static void Debug(this PartModule pm, string msg) => Utils.Debug($"{pm.GetID()}: {msg}");
        public static void Info(this PartModule pm, string msg) => Utils.Info($"{pm.GetID()}: {msg}");
        public static void Warning(this PartModule pm, string msg) => Utils.Warning($"{pm.GetID()}: {msg}");
        public static void Error(this PartModule pm, string msg) => Utils.Error($"{pm.GetID()}: {msg}");

        public static void Log(this PartModule pm, string msg, params object[] args) =>
            Utils.Log($"{pm.GetID()}: {msg}", args);
    }
}
