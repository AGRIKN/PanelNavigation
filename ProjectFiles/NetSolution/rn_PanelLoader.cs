#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using System.Linq;
#endregion

public class rn_PanelLoader : BaseNetLogic
{
    private IUAVariable indexVariable;
    private PanelLoader panelLoader;


    // Panel names MUST match those registered in the PanelLoader's Panel list
    private string[] panelNames = { "Welcome", "Settings", "Recipes", "Alarms" };

    public override void Start()
    {
        indexVariable = Project.Current.GetVariable("Model/CurrentPanelIndex");
        if (indexVariable == null)
        {
            Log.Error("indexVariable is NULL! Check if 'CurrentPanelIndex' exists and is of type Int32.");
            return;
        }

        // Use full path because PanelLoader is under MainWindow, not next to this logic
        panelLoader = Project.Current.Get<PanelLoader>("UI/MainWindow/PanelLoader");
        if (panelLoader == null)
        {
            Log.Error("panelLoader is NULL! Check if 'PanelLoader' exists at 'UI/MainWindow/PanelLoader'.");
            return;
        }

        UpdatePanel(); // only call if both are valid
    }

    [ExportMethod]
    public void GoNext()
    {
        Log.Info("GoNext called.");

        int index = (int)indexVariable.Value;
        if (index < panelNames.Length - 1)
        {
            indexVariable.Value = index + 1;
            UpdatePanel();
        }
    }

    [ExportMethod]
    public void GoBack()
    {
        Log.Info("GoBack called.");

        int index = (int)indexVariable.Value;
        if (index > 0)
        {
            indexVariable.Value = index - 1;
            UpdatePanel();
        }
    }

    private void UpdatePanel()
    {
        int index = (int)indexVariable.Value;

        if (index < 0 || index >= panelNames.Length)
        {
            Log.Error($"Invalid panel index: {index}");
            return;
        }

        if (panelLoader == null)
        {
            Log.Error("panelLoader is NULL in UpdatePanel(). Skipping panel update.");
            return;
        }

        string targetPanelName = panelNames[index];
        var panelTypeToLoad = Project.Current.Get<PanelType>($"UI/MyPanels/{targetPanelName}");

        if (panelTypeToLoad == null)
        {
            Log.Error($"Panel type '{targetPanelName}' not found in UI/MyPanels.");
            return;
        }

        panelLoader.GetVariable("Panel").Value = panelTypeToLoad.NodeId;
    
        Log.Info($"[UpdatePanel] index: {index}, loaded panel type: {targetPanelName}");
    }
}
