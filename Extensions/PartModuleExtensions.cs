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
            Utils.Message(6,
                "WARNING: {0}.\n" + "Configuration of \"{1}\" is INVALID.",
                string.Format(msg, args),
                pm.Title());
            pm.enabled = pm.isEnabled = false;
        }

        public static void Log(this PartModule pm, string msg) => Utils.Log($"{pm.GetID()}: {msg}");

        public static void Log(this PartModule pm, string msg, params object[] args) =>
            Utils.Log($"{pm.GetID()}: {msg}", args);
    }
}
