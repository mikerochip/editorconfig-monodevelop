using MonoDevelop.Core;

namespace EditorConfig.Addin
{
    class PropertyMgr
    {
        public const string ScopePrefix = "EditorConfig";
        public static readonly string LetEolApplyKey = $"{ScopePrefix}.LetEolApply";

        static PropertyMgr instance;


        public static void Initialize()
        {
            if (instance != null)
                return;

            instance = new PropertyMgr();
            instance.InitializeImpl();
        }

        public static PropertyMgr Get()
        {
            return instance;
        }


        public void LoadLetEolApply()
        {
            Engine.LetEolApply = PropertyService.Get<bool>(LetEolApplyKey);
        }

        public void SaveLetEolApply()
        {
            PropertyService.Set(LetEolApplyKey, Engine.LetEolApply);
        }


        void InitializeImpl()
        {
            LoadLetEolApply();
        }
    }
}
