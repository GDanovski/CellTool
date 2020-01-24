
// Helpers/Settings.cs This file was automatically added when you installed the Settings Plugin. If you are not using a PCL then comment this file back in to use it.
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;

namespace Cell_Tool_3.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string SettingsKey = "settings_key";
        private static readonly string SettingsDefault = string.Empty;

        #endregion


        public static string GeneralSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SettingsKey, value);
            }
        }

        #region MySettings

        private static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
        public static void LoadSettings()
        {
            if (!IsRunningOnMono()) return;

            Properties.Settings MSSettings = Properties.Settings.Default;
            //Properties.Settings.Default.Reset();
            //Properties.Settings.Default.Save();

            if (!MSSettings.ShowLicense) return; //Prevends overwriting on first loading

            try
            {
                MSSettings.TrialActive = LoadSetting(MSSettings.TrialActive, TrialActive);
                MSSettings.UpdateSettings = LoadSetting(MSSettings.UpdateSettings, UpdateSettings);
                MSSettings.ShowLicense = LoadSetting(MSSettings.ShowLicense, ShowLicense);
                MSSettings.BlockProgram = LoadSetting(MSSettings.BlockProgram, BlockProgram);
                MSSettings.IncorrectPass = LoadSetting(MSSettings.IncorrectPass, IncorrectPass);
                MSSettings.AutoProtocolSettings = LoadSetting(MSSettings.AutoProtocolSettings, AutoProtocolSettings);
                MSSettings.AccPass = LoadSetting(MSSettings.AccPass, AccPass);
                MSSettings.CTChart_SeriesHeight = LoadSetting(MSSettings.CTChart_SeriesHeight, CTChart_SeriesHeight);
                MSSettings.BandCVis = LoadSetting(MSSettings.BandCVis, BandCVis);
                MSSettings.CTChart_Functions = LoadSetting(MSSettings.CTChart_Functions, CTChart_Functions);
                MSSettings.DataSourcesPanelValues = LoadSetting(MSSettings.DataSourcesPanelValues, DataSourcesPanelValues);
                MSSettings.DataSourcesPanelVisible = LoadSetting(MSSettings.DataSourcesPanelVisible, DataSourcesPanelVisible);
                MSSettings.CTChart_PropertiesVis = LoadSetting(MSSettings.CTChart_PropertiesVis, CTChart_PropertiesVis);
                MSSettings.HotKeys = LoadSetting(MSSettings.HotKeys, HotKeys);
                MSSettings.CTChart_SeriesVis = LoadSetting(MSSettings.CTChart_SeriesVis, CTChart_SeriesVis);
                MSSettings.Meta = LoadSetting(MSSettings.Meta, Meta);
                MSSettings.MetaVis = LoadSetting(MSSettings.MetaVis, MetaVis);
                MSSettings.OldWorkDir = LoadSetting(MSSettings.OldWorkDir, OldWorkDir);
                MSSettings.CustomColors = LoadSetting(MSSettings.CustomColors, CustomColors);
                MSSettings.PropertiesPanelWidth = LoadSetting(MSSettings.PropertiesPanelWidth, PropertiesPanelWidth);
                MSSettings.AccList = LoadSetting(MSSettings.AccList, AccList);
                MSSettings.ResultsExtractorFilters = LoadSetting(MSSettings.ResultsExtractorFilters, ResultsExtractorFilters);
                MSSettings.ResultsExtractorSizes = LoadSetting(MSSettings.ResultsExtractorSizes, ResultsExtractorSizes);
                MSSettings.RoiManHeight = LoadSetting(MSSettings.RoiManHeight, RoiManHeight);
                MSSettings.RoiManVis = LoadSetting(MSSettings.RoiManVis, RoiManVis);
                MSSettings.SegmentDataPanelVis = LoadSetting(MSSettings.SegmentDataPanelVis, SegmentDataPanelVis);
                MSSettings.SegmentHistPanelHeight = LoadSetting(MSSettings.SegmentHistPanelHeight, SegmentHistPanelHeight);
                MSSettings.SegmentHistPanelVis = LoadSetting(MSSettings.SegmentHistPanelVis, SegmentHistPanelVis);
                MSSettings.SegmentLibPanelVis = LoadSetting(MSSettings.SegmentLibPanelVis, SegmentLibPanelVis);
                MSSettings.SegmentSpotDetPanelVis = LoadSetting(MSSettings.SegmentSpotDetPanelVis, SegmentSpotDetPanelVis);
                MSSettings.PropertiesPanelVisible = LoadSetting(MSSettings.PropertiesPanelVisible, PropertiesPanelVisible);
                MSSettings.SmartBtns = LoadSetting(MSSettings.SmartBtns, SmartBtns);
                MSSettings.SolverFunctions = LoadSetting(MSSettings.SolverFunctions, SolverFunctions);
                MSSettings.TrackingVis = LoadSetting(MSSettings.TrackingVis, TrackingVis);
                MSSettings.TreeViewContent = LoadSetting(MSSettings.TreeViewContent, TreeViewContent);
                MSSettings.TreeViewSize = LoadSetting(MSSettings.TreeViewSize, TreeViewSize);
                MSSettings.TreeViewVisible = LoadSetting(MSSettings.TreeViewVisible, TreeViewVisible);
                MSSettings.ProtocolSettingsList = LoadSetting(MSSettings.ProtocolSettingsList, ProtocolSettingsList);
                MSSettings.VBoxVisible = LoadSetting(MSSettings.VBoxVisible, VBoxVisible);
                //MSSettings.EndTrialDate = LoadSetting(MSSettings.EndTrialDate, EndTrialDate);
            }
            catch { }
			try {
            	MSSettings.Save();
			} catch {
			}
        }
        private static bool LoadSetting(bool MSsetting,string setting)
        {
            return (bool.Parse(setting));
        }
        private static byte LoadSetting(byte MSsetting,string setting)
        {
            return (byte.Parse(setting));
        }
        private static System.Collections.Specialized.StringCollection LoadSetting(System.Collections.Specialized.StringCollection MSsetting, string setting)
        {
            MSsetting.Clear();

            foreach(string val in setting.Split(new string[] { "<sp>"}, StringSplitOptions.None))
            {
                MSsetting.Add(val);
            }

            return MSsetting;
        }

        public static void SaveSettings()
        {
            if (!IsRunningOnMono()) return;

            Properties.Settings MSSettings = Properties.Settings.Default;
           
            TrialActive = SaveSetting(MSSettings.TrialActive);
            UpdateSettings = SaveSetting(MSSettings.UpdateSettings);
            ShowLicense = SaveSetting(MSSettings.ShowLicense);
            BlockProgram = SaveSetting(MSSettings.BlockProgram);
            IncorrectPass = SaveSetting(MSSettings.IncorrectPass);
            AutoProtocolSettings = SaveSetting(MSSettings.AutoProtocolSettings);
            AccPass = SaveSetting(MSSettings.AccPass);
            CTChart_SeriesHeight = SaveSetting(MSSettings.CTChart_SeriesHeight);
            BandCVis = SaveSetting(MSSettings.BandCVis);
            CTChart_Functions = SaveSetting(MSSettings.CTChart_Functions);
            DataSourcesPanelValues = SaveSetting(MSSettings.DataSourcesPanelValues);
            DataSourcesPanelVisible = SaveSetting(MSSettings.DataSourcesPanelVisible);
            CTChart_PropertiesVis = SaveSetting(MSSettings.CTChart_PropertiesVis);
            HotKeys = SaveSetting(MSSettings.HotKeys);
            CTChart_SeriesVis = SaveSetting(MSSettings.CTChart_SeriesVis);
            Meta = SaveSetting(MSSettings.Meta);
            MetaVis = SaveSetting(MSSettings.MetaVis);
            OldWorkDir = SaveSetting(MSSettings.OldWorkDir);
            CustomColors = SaveSetting(MSSettings.CustomColors);
            PropertiesPanelWidth = SaveSetting(MSSettings.PropertiesPanelWidth);
            AccList = SaveSetting(MSSettings.AccList);
            ResultsExtractorFilters = SaveSetting(MSSettings.ResultsExtractorFilters);
            ResultsExtractorSizes = SaveSetting(MSSettings.ResultsExtractorSizes);
            RoiManHeight = SaveSetting(MSSettings.RoiManHeight);
            RoiManVis = SaveSetting(MSSettings.RoiManVis);
            SegmentDataPanelVis = SaveSetting(MSSettings.SegmentDataPanelVis);
            SegmentHistPanelHeight = SaveSetting(MSSettings.SegmentHistPanelHeight);
            SegmentHistPanelVis = SaveSetting(MSSettings.SegmentHistPanelVis);
            SegmentLibPanelVis = SaveSetting(MSSettings.SegmentLibPanelVis);
            SegmentSpotDetPanelVis = SaveSetting(MSSettings.SegmentSpotDetPanelVis);
            PropertiesPanelVisible = SaveSetting(MSSettings.PropertiesPanelVisible);
            SmartBtns = SaveSetting(MSSettings.SmartBtns);
            SolverFunctions = SaveSetting(MSSettings.SolverFunctions);
            TrackingVis = SaveSetting(MSSettings.TrackingVis);
            TreeViewContent = SaveSetting(MSSettings.TreeViewContent);
            TreeViewSize = SaveSetting(MSSettings.TreeViewSize);
            TreeViewVisible = SaveSetting(MSSettings.TreeViewVisible);
            ProtocolSettingsList = SaveSetting(MSSettings.ProtocolSettingsList);
            VBoxVisible = SaveSetting(MSSettings.VBoxVisible);
            //EndTrialDate = SaveSetting(MSSettings.EndTrialDate);
        }

        private static string SaveSetting(bool MSsetting)
        {
            return MSsetting.ToString();
        }
        private static string SaveSetting(byte MSsetting)
        {
            return MSsetting.ToString();
        }
        private static string SaveSetting(System.Collections.Specialized.StringCollection MSsetting)
        {
            string[] vals = new string[MSsetting.Count];
            for (int i = 0; i < vals.Length; i++)
                vals[i] = MSsetting[i];

            return string.Join("<sp>",vals);
        }

        #region SettingsConstants
        private const string TrialActiveKey = "TrialActive";
        private const string UpdateSettingsKey = "UpdateSettings";
        private const string ShowLicenseKey = "ShowLicense";
        private const string BlockProgramKey = "BlockProgram";
        private const string IncorrectPassKey = "IncorrectPass";
        private const string AutoProtocolSettingsKey = "AutoProtocolSettings";
        private const string AccPassKey = "AccPass";
        private const string CTChart_SeriesHeightKey = "CTChart_SeriesHeight";
        private const string BandCVisKey = "BandCVis";
        private const string CTChart_FunctionsKey = "CTChart_Functions";
        private const string DataSourcesPanelValuesKey = "DataSourcesPanelValues";
        private const string DataSourcesPanelVisibleKey = "DataSourcesPanelVisible";
        private const string CTChart_PropertiesVisKey = "CTChart_PropertiesVis";
        private const string HotKeysKey = "HotKeys";
        private const string CTChart_SeriesVisKey = "CTChart_SeriesVis";
        private const string MetaKey = "Meta";
        private const string MetaVisKey = "MetaVis";
        private const string OldWorkDirKey = "OldWorkDir";
        private const string CustomColorsKey = "CustomColors";
        private const string PropertiesPanelWidthKey = "PropertiesPanelWidth";
        private const string AccListKey = "AccList";
        private const string ResultsExtractorFiltersKey = "ResultsExtractorFilters";
        private const string ResultsExtractorSizesKey = "ResultsExtractorSizes";
        private const string RoiManHeightKey = "RoiManHeight";
        private const string RoiManVisKey = "RoiManVis";
        private const string SegmentDataPanelVisKey = "SegmentDataPanelVis";
        private const string SegmentHistPanelHeightKey = "SegmentHistPanelHeight";
        private const string SegmentHistPanelVisKey = "SegmentHistPanelVis";
        private const string SegmentLibPanelVisKey = "SegmentLibPanelVis";
        private const string SegmentSpotDetPanelVisKey = "SegmentSpotDetPanelVis";
        private const string PropertiesPanelVisibleKey = "PropertiesPanelVisible";
        private const string SmartBtnsKey = "SmartBtns";
        private const string SolverFunctionsKey = "SolverFunctions";
        private const string TrackingVisKey = "TrackingVis";
        private const string TreeViewContentKey = "TreeViewContent";
        private const string TreeViewSizeKey = "TreeViewSize";
        private const string TreeViewVisibleKey = "TreeViewVisible";
        private const string ProtocolSettingsListKey = "ProtocolSettingsList";
        private const string VBoxVisibleKey = "VBoxVisible";
        //private const string EndTrialDateKey = "EndTrialDate";

        private static readonly string TrialActiveDefault = "False";
        private static readonly string UpdateSettingsDefault = "True";
        private static readonly string ShowLicenseDefault = "True";
        private static readonly string BlockProgramDefault = "False";
        private static readonly string IncorrectPassDefault = "0";
        private static readonly string AutoProtocolSettingsDefault = "true";
        private static readonly string AccPassDefault = "123456";
        private static readonly string CTChart_SeriesHeightDefault = "200";
        private static readonly string BandCVisDefault = "y";
        private static readonly string CTChart_FunctionsDefault = "@";
        private static readonly string DataSourcesPanelValuesDefault = "300";
        private static readonly string DataSourcesPanelVisibleDefault = "y";
        private static readonly string CTChart_PropertiesVisDefault = "y";
        private static readonly string HotKeysDefault = "@";
        private static readonly string CTChart_SeriesVisDefault = "y";
        private static readonly string MetaDefault = "130";
        private static readonly string MetaVisDefault = "y";
        private static readonly string OldWorkDirDefault = "@";
        private static readonly string CustomColorsDefault = "@";
        private static readonly string PropertiesPanelWidthDefault = "300";
        private static readonly string AccListDefault = "Admin";
        private static readonly string ResultsExtractorFiltersDefault = "@";
        private static readonly string ResultsExtractorSizesDefault = "@";
        private static readonly string RoiManHeightDefault = "300";
        private static readonly string RoiManVisDefault = "y";
        private static readonly string SegmentDataPanelVisDefault = "y";
        private static readonly string SegmentHistPanelHeightDefault = "200";
        private static readonly string SegmentHistPanelVisDefault = "y";
        private static readonly string SegmentLibPanelVisDefault = "y";
        private static readonly string SegmentSpotDetPanelVisDefault = "y";
        private static readonly string PropertiesPanelVisibleDefault = "y";
        private static readonly string SmartBtnsDefault = "@";
        private static readonly string SolverFunctionsDefault = "@";
        private static readonly string TrackingVisDefault = "y";
        private static readonly string TreeViewContentDefault = "@";
        private static readonly string TreeViewSizeDefault = "200";
        private static readonly string TreeViewVisibleDefault = "y";
        private static readonly string ProtocolSettingsListDefault = "@";
        private static readonly string VBoxVisibleDefault = "y";

        #endregion SettingsConstants


        public static string TrialActive
        {
            get
            {
                return AppSettings.GetValueOrDefault(TrialActiveKey, TrialActiveDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(TrialActiveKey, value);
            }
        }
        public static string UpdateSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault(UpdateSettingsKey, UpdateSettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(UpdateSettingsKey, value);
            }
        }
        public static string ShowLicense
        {
            get
            {
                return AppSettings.GetValueOrDefault(ShowLicenseKey, ShowLicenseDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(ShowLicenseKey, value);
            }
        }
        public static string BlockProgram
        {
            get
            {
                return AppSettings.GetValueOrDefault(BlockProgramKey, BlockProgramDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(BlockProgramKey, value);
            }
        }
        public static string IncorrectPass
        {
            get
            {
                return AppSettings.GetValueOrDefault(IncorrectPassKey, IncorrectPassDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IncorrectPassKey, value);
            }
        }
        public static string AutoProtocolSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault(AutoProtocolSettingsKey, AutoProtocolSettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AutoProtocolSettingsKey, value);
            }
        }
        public static string AccPass
        {
            get
            {
                return AppSettings.GetValueOrDefault(AccPassKey, AccPassDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AccPassKey, value);
            }
        }
        public static string CTChart_SeriesHeight
        {
            get
            {
                return AppSettings.GetValueOrDefault(CTChart_SeriesHeightKey, CTChart_SeriesHeightDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(CTChart_SeriesHeightKey, value);
            }
        }
        public static string BandCVis
        {
            get
            {
                return AppSettings.GetValueOrDefault(BandCVisKey, BandCVisDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(BandCVisKey, value);
            }
        }
        public static string CTChart_Functions
        {
            get
            {
                return AppSettings.GetValueOrDefault(CTChart_FunctionsKey, CTChart_FunctionsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(CTChart_FunctionsKey, value);
            }
        }
        public static string DataSourcesPanelValues
        {
            get
            {
                return AppSettings.GetValueOrDefault(DataSourcesPanelValuesKey, DataSourcesPanelValuesDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(DataSourcesPanelValuesKey, value);
            }
        }
        public static string DataSourcesPanelVisible
        {
            get
            {
                return AppSettings.GetValueOrDefault(DataSourcesPanelVisibleKey, DataSourcesPanelVisibleDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(DataSourcesPanelVisibleKey, value);
            }
        }
        public static string CTChart_PropertiesVis
        {
            get
            {
                return AppSettings.GetValueOrDefault(CTChart_PropertiesVisKey, CTChart_PropertiesVisDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(CTChart_PropertiesVisKey, value);
            }
        }
        public static string HotKeys
        {
            get
            {
                return AppSettings.GetValueOrDefault(HotKeysKey, HotKeysDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(HotKeysKey, value);
            }
        }
        public static string CTChart_SeriesVis
        {
            get
            {
                return AppSettings.GetValueOrDefault(CTChart_SeriesVisKey, CTChart_SeriesVisDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(CTChart_SeriesVisKey, value);
            }
        }
        public static string Meta
        {
            get
            {
                return AppSettings.GetValueOrDefault(MetaKey, MetaDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(MetaKey, value);
            }
        }
        public static string MetaVis
        {
            get
            {
                return AppSettings.GetValueOrDefault(MetaVisKey, MetaVisDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(MetaVisKey, value);
            }
        }
        public static string OldWorkDir
        {
            get
            {
                return AppSettings.GetValueOrDefault(OldWorkDirKey, OldWorkDirDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(OldWorkDirKey, value);
            }
        }
        public static string CustomColors
        {
            get
            {
                return AppSettings.GetValueOrDefault(CustomColorsKey, CustomColorsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(CustomColorsKey, value);
            }
        }
        public static string PropertiesPanelWidth
        {
            get
            {
                return AppSettings.GetValueOrDefault(PropertiesPanelWidthKey, PropertiesPanelWidthDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(PropertiesPanelWidthKey, value);
            }
        }
        public static string AccList
        {
            get
            {
                return AppSettings.GetValueOrDefault(AccListKey, AccListDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AccListKey, value);
            }
        }
        public static string ResultsExtractorFilters
        {
            get
            {
                return AppSettings.GetValueOrDefault(ResultsExtractorFiltersKey, ResultsExtractorFiltersDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(ResultsExtractorFiltersKey, value);
            }
        }
        public static string ResultsExtractorSizes
        {
            get
            {
                return AppSettings.GetValueOrDefault(ResultsExtractorSizesKey, ResultsExtractorSizesDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(ResultsExtractorSizesKey, value);
            }
        }
        public static string RoiManHeight
        {
            get
            {
                return AppSettings.GetValueOrDefault(RoiManHeightKey, RoiManHeightDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(RoiManHeightKey, value);
            }
        }
        public static string RoiManVis
        {
            get
            {
                return AppSettings.GetValueOrDefault(RoiManVisKey, RoiManVisDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(RoiManVisKey, value);
            }
        }
        public static string SegmentDataPanelVis
        {
            get
            {
                return AppSettings.GetValueOrDefault(SegmentDataPanelVisKey, SegmentDataPanelVisDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SegmentDataPanelVisKey, value);
            }
        }
        public static string SegmentHistPanelHeight
        {
            get
            {
                return AppSettings.GetValueOrDefault(SegmentHistPanelHeightKey, SegmentHistPanelHeightDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SegmentHistPanelHeightKey, value);
            }
        }
        public static string SegmentHistPanelVis
        {
            get
            {
                return AppSettings.GetValueOrDefault(SegmentHistPanelVisKey, SegmentHistPanelVisDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SegmentHistPanelVisKey, value);
            }
        }
        public static string SegmentLibPanelVis
        {
            get
            {
                return AppSettings.GetValueOrDefault(SegmentLibPanelVisKey, SegmentLibPanelVisDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SegmentLibPanelVisKey, value);
            }
        }
        public static string SegmentSpotDetPanelVis
        {
            get
            {
                return AppSettings.GetValueOrDefault(SegmentSpotDetPanelVisKey, SegmentSpotDetPanelVisDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SegmentSpotDetPanelVisKey, value);
            }
        }
        public static string PropertiesPanelVisible
        {
            get
            {
                return AppSettings.GetValueOrDefault(PropertiesPanelVisibleKey, PropertiesPanelVisibleDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(PropertiesPanelVisibleKey, value);
            }
        }
        public static string SmartBtns
        {
            get
            {
                return AppSettings.GetValueOrDefault(SmartBtnsKey, SmartBtnsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SmartBtnsKey, value);
            }
        }
        public static string SolverFunctions
        {
            get
            {
                return AppSettings.GetValueOrDefault(SolverFunctionsKey, SolverFunctionsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SolverFunctionsKey, value);
            }
        }
        public static string TrackingVis
        {
            get
            {
                return AppSettings.GetValueOrDefault(TrackingVisKey, TrackingVisDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(TrackingVisKey, value);
            }
        }
        public static string TreeViewContent
        {
            get
            {
                return AppSettings.GetValueOrDefault(TreeViewContentKey, TreeViewContentDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(TreeViewContentKey, value);
            }
        }
        public static string TreeViewSize
        {
            get
            {
                return AppSettings.GetValueOrDefault(TreeViewSizeKey, TreeViewSizeDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(TreeViewSizeKey, value);
            }
        }
        public static string TreeViewVisible
        {
            get
            {
                return AppSettings.GetValueOrDefault(TreeViewVisibleKey, TreeViewVisibleDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(TreeViewVisibleKey, value);
            }
        }
        public static string ProtocolSettingsList
        {
            get
            {
                return AppSettings.GetValueOrDefault(ProtocolSettingsListKey, ProtocolSettingsListDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(ProtocolSettingsListKey, value);
            }
        }
        public static string VBoxVisible
        {
            get
            {
                return AppSettings.GetValueOrDefault(VBoxVisibleKey, VBoxVisibleDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(VBoxVisibleKey, value);
            }
        }
        /*
        public static string EndTrialDate
        {
            get
            {
                return AppSettings.GetValueOrDefault(EndTrialDateKey, EndTrialDateDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(EndTrialDateKey, value);
            }
        }*/
        #endregion My Settings
    }
}