/* ============================================================================
   BSOOPCUABrowser - Designs + Button in BSOiPlusStudio (TabConfigList)
   ----------------------------------------------------------------------------
   Re-runnable script. Creates the ACClassDesign rows (WPF XMLDesign + Avalonia
   XMLDesign2) for the global BSO "BSOOPCUABrowser" (namespace gip.core.communication)
   and adds the "OPC UA Browser" button to the TabConfigList design of BSOiPlusStudio.

   PREREQUISITE: The ACClass row for BSOOPCUABrowser is created automatically by the
   iPlus class-sync at client start (assembly gip.core.communication). Therefore:
   1) Deploy the new gip.core.communication.dll
   2) Start the client ONCE (so the ACClass row is registered)
   3) Run this script
   4) Restart the client (designs are cached)

   Targets: IPCDrivesV5 (works for any iPlus V5 database)
   ============================================================================ */

SET NOCOUNT ON;

DECLARE @ClassID uniqueidentifier = (SELECT ACClassID FROM ACClass WHERE ACIdentifier = 'BSOOPCUABrowser');

IF @ClassID IS NULL
BEGIN
    PRINT 'ACClass ''BSOOPCUABrowser'' not found. Start the client once with the new gip.core.communication.dll so the class gets registered, then re-run this script.';
    RETURN;
END

/* ---------------------------------------------------------------------------
   1) Design "Mainlayout" (docking: Browser tab + Messages auto-hide)
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM ACClassDesign WHERE ACClassID = @ClassID AND ACIdentifier = 'Mainlayout')
BEGIN
    INSERT INTO ACClassDesign (ACClassDesignID, ACClassID, ACIdentifier, ACCaptionTranslation, XMLDesign, XMLDesign2, XMLDesign2UpdateDate,
                               ACKindIndex, ACUsageIndex, ValueTypeACClassID, SortIndex, IsDefault, BranchNo,
                               InsertName, InsertDate, UpdateName, UpdateDate)
    SELECT NEWID(), @ClassID, 'Mainlayout', 'en{' + CHAR(39) + 'Mainlayout' + CHAR(39) + '}de{' + CHAR(39) + 'Mainlayout' + CHAR(39) + '}',
        N'<?xml version="1.0" encoding="utf-16"?>
<vb:VBDockingManager xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:vb="http://www.iplus-framework.com/xaml" xmlns:vbv="http://www.iplus-framework.com/visual/xaml">
    <vb:VBDesign x:Name="Browser" VBContent="*Browser" vb:VBDockingManager.IsCloseableBSORoot="False" vb:VBDockingManager.Container="TabItem" vb:VBDockingManager.DockState="Tabbed" vb:VBDockingManager.DockPosition="Bottom" vb:VBDockingManager.RibbonBarVisibility="Hidden" vb:VBDockingManager.WindowSize="0,0" />
    <vb:VBDesign VBContent="*Messages" ACCaption="Messages" vb:VBDockingManager.IsCloseableBSORoot="False" vb:VBDockingManager.Container="DockableWindow" vb:VBDockingManager.DockState="AutoHideButton" vb:VBDockingManager.DockPosition="Right" vb:VBDockingManager.RibbonBarVisibility="Hidden" vb:VBDockingManager.WindowSize="0,0" />
</vb:VBDockingManager>',
        N'<?xml version="1.0" encoding="utf-16"?>
<vb:VBDockingManager xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:vb="http://www.iplus-framework.com/axaml" xmlns:vbv="http://www.iplus-framework.com/visual/axaml">
    <vb:VBDesign x:Name="Browser" VBContent="*Browser" vb:VBDockingManager.IsCloseableBSORoot="False" vb:VBDockingManager.Container="TabItem" vb:VBDockingManager.DockState="Tabbed" vb:VBDockingManager.DockPosition="Bottom" vb:VBDockingManager.RibbonBarVisibility="Hidden" vb:VBDockingManager.WindowSize="0,0" />
    <vb:VBDesign VBContent="*Messages" ACCaption="Messages" vb:VBDockingManager.IsCloseableBSORoot="False" vb:VBDockingManager.Container="DockableWindow" vb:VBDockingManager.DockState="AutoHideButton" vb:VBDockingManager.DockPosition="Right" vb:VBDockingManager.RibbonBarVisibility="Hidden" vb:VBDockingManager.WindowSize="0,0" />
</vb:VBDockingManager>',
        GETDATE(),
        d.ACKindIndex, d.ACUsageIndex, d.ValueTypeACClassID, 0, 0, d.BranchNo,
        'GIP', GETDATE(), 'GIP', GETDATE()
    FROM ACClassDesign d
    WHERE d.ACClassID = (SELECT ACClassID FROM ACClass WHERE ACIdentifier = 'BSOiPlusExport') AND d.ACIdentifier = 'Mainlayout';
END
GO

/* ---------------------------------------------------------------------------
   2) Design "Browser" (session combo, subscription caption, checkbox tree, buttons)
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM ACClassDesign WHERE ACClassID = @ClassID AND ACIdentifier = 'Browser')
BEGIN
    INSERT INTO ACClassDesign (ACClassDesignID, ACClassID, ACIdentifier, ACCaptionTranslation, XMLDesign, XMLDesign2, XMLDesign2UpdateDate,
                               ACKindIndex, ACUsageIndex, ValueTypeACClassID, SortIndex, IsDefault, BranchNo,
                               InsertName, InsertDate, UpdateName, UpdateDate)
    SELECT NEWID(), @ClassID, 'Browser', 'en{' + CHAR(39) + 'Browser' + CHAR(39) + '}de{' + CHAR(39) + 'Browser' + CHAR(39) + '}',
        N'<?xml version="1.0" encoding="utf-16"?>
<vb:VBGrid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:vb="http://www.iplus-framework.com/xaml" xmlns:vbv="http://www.iplus-framework.com/visual/xaml">
  <Grid.RowDefinitions>
    <RowDefinition Height="30" />
    <RowDefinition Height="30" />
    <RowDefinition />
    <RowDefinition Height="35" />
    <RowDefinition Height="35" />
  </Grid.RowDefinitions>
  <vb:VBFrame Grid.RowSpan="5" />
  <vb:VBGrid Grid.Row="0">
    <Grid.ColumnDefinitions>
      <ColumnDefinition />
      <ColumnDefinition Width="50" />
    </Grid.ColumnDefinitions>
    <vb:VBComboBox VBContent="CurrentSession" ShowCaption="false" Name="CurrentSession_0" />
    <vb:VBButton VBContent="!RefreshSessionList" Grid.Column="1" Name="RefreshSessionList_0" />
  </vb:VBGrid>
  <vb:VBTextBox Grid.Row="1" VBContent="CurrentSubscriptionClass\ACCaption" DisabledModes="Disabled" ShowCaption="false" Name="CurrentSubscriptionClass_0" />
  <vb:VBTreeView Grid.Row="2" VBContent="CurrentBrowseItem" VBSource="BrowseTreeRoot" ShowCaption="false" VBShowColumns="ItemText" VBChilds="VisibleItemsT" VBTreeViewExpandMethod="!OnNodeExpand" Name="CurrentBrowseItem_0" />
  <vb:VBButton Grid.Row="3" VBContent="!Browse" Name="Browse_0" />
  <vb:VBButton Grid.Row="4" VBContent="!AddToSubscription" Name="AddToSubscription_0" />
</vb:VBGrid>',
        N'<?xml version="1.0" encoding="utf-16"?>
<vb:VBGrid xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:vb="http://www.iplus-framework.com/axaml" xmlns:vbv="http://www.iplus-framework.com/visual/axaml">
  <Grid.RowDefinitions>
    <RowDefinition Height="30" />
    <RowDefinition Height="30" />
    <RowDefinition />
    <RowDefinition Height="35" />
    <RowDefinition Height="35" />
  </Grid.RowDefinitions>
  <vb:VBFrame Grid.RowSpan="5" />
  <vb:VBGrid Grid.Row="0">
    <Grid.ColumnDefinitions>
      <ColumnDefinition />
      <ColumnDefinition Width="50" />
    </Grid.ColumnDefinitions>
    <vb:VBComboBox VBContent="CurrentSession" ShowCaption="false" Name="CurrentSession_0" />
    <vb:VBButton VBContent="!RefreshSessionList" Grid.Column="1" Name="RefreshSessionList_0" />
  </vb:VBGrid>
  <vb:VBTextBox Grid.Row="1" VBContent="CurrentSubscriptionClass\ACCaption" DisabledModes="Disabled" ShowCaption="false" Name="CurrentSubscriptionClass_0" />
  <vb:VBTreeView Grid.Row="2" VBContent="CurrentBrowseItem" VBSource="BrowseTreeRoot" ShowCaption="false" VBShowColumns="ItemText" VBChilds="VisibleItemsT" VBTreeViewExpandMethod="!OnNodeExpand" Name="CurrentBrowseItem_0" />
  <vb:VBButton Grid.Row="3" VBContent="!Browse" Name="Browse_0" />
  <vb:VBButton Grid.Row="4" VBContent="!AddToSubscription" Name="AddToSubscription_0" />
</vb:VBGrid>',
        GETDATE(),
        d.ACKindIndex, d.ACUsageIndex, d.ValueTypeACClassID, 1, 0, d.BranchNo,
        'GIP', GETDATE(), 'GIP', GETDATE()
    FROM ACClassDesign d
    WHERE d.ACClassID = (SELECT ACClassID FROM ACClass WHERE ACIdentifier = 'BSOiPlusExport') AND d.ACIdentifier = 'ExportTree';
END
GO

/* ---------------------------------------------------------------------------
   3) Design "Messages" (standard message list)
   --------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM ACClassDesign WHERE ACClassID = @ClassID AND ACIdentifier = 'Messages')
BEGIN
    INSERT INTO ACClassDesign (ACClassDesignID, ACClassID, ACIdentifier, ACCaptionTranslation, XMLDesign, XMLDesign2, XMLDesign2UpdateDate,
                               ACKindIndex, ACUsageIndex, ValueTypeACClassID, SortIndex, IsDefault, BranchNo,
                               InsertName, InsertDate, UpdateName, UpdateDate)
    SELECT NEWID(), @ClassID, 'Messages', 'en{' + CHAR(39) + 'Messages' + CHAR(39) + '}de{' + CHAR(39) + 'Meldungen' + CHAR(39) + '}',
        N'<?xml version="1.0" encoding="utf-16"?>
<vb:VBDockPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:vbs="http://www.iplus-framework.com/shapes/xaml" xmlns:vb="http://www.iplus-framework.com/xaml" xmlns:vbm="http://www.iplus-framework.com/manager/xaml" xmlns:vbr="http://www.iplus-framework.com/report/xaml" xmlns:vbv="http://www.iplus-framework.com/visual/xaml">
    <vb:VBDataGrid VBContent="CurrentMsg" VBShowColumns="MessageLevel,ACIdentifier,Message" DisabledModes="Disabled"></vb:VBDataGrid>
</vb:VBDockPanel>',
        NULL,
        NULL,
        d.ACKindIndex, d.ACUsageIndex, d.ValueTypeACClassID, 999, 0, d.BranchNo,
        'GIP', GETDATE(), 'GIP', GETDATE()
    FROM ACClassDesign d
    WHERE d.ACClassID = (SELECT ACClassID FROM ACClass WHERE ACIdentifier = 'BSOiPlusExport') AND d.ACIdentifier = 'Messages';
END
GO

/* ---------------------------------------------------------------------------
   4) Button "OPC UA Browser" in BSOiPlusStudio.TabConfigList (WPF + Avalonia)
      - adds a RowDefinition to the ControlModePointProperty grid
      - adds the button below "AutoRenameVariables"
   --------------------------------------------------------------------------- */
DECLARE @xml nvarchar(max);
DECLARE @studioID uniqueidentifier = (SELECT ACClassID FROM ACClass WHERE ACIdentifier = 'BSOiPlusStudio');

-- 4a) WPF (XMLDesign)
SET @xml = (SELECT CAST(d.XMLDesign AS nvarchar(max)) FROM ACClassDesign d WHERE d.ACClassID = @studioID AND d.ACIdentifier = 'TabConfigList');
IF @xml IS NOT NULL AND @xml NOT LIKE '%StartOPCUABrowser%'
BEGIN
    -- add one row to the RowDefinitions of the ControlModePointProperty grid (unique 3-row block)
    SET @xml = REPLACE(@xml, N'            <RowDefinition Height="*"></RowDefinition>
            <RowDefinition Height="40"></RowDefinition>
            <RowDefinition Height="40"></RowDefinition>
        </Grid.RowDefinitions>', N'            <RowDefinition Height="*"></RowDefinition>
            <RowDefinition Height="40"></RowDefinition>
            <RowDefinition Height="40"></RowDefinition>
            <RowDefinition Height="40"></RowDefinition>
        </Grid.RowDefinitions>');
    -- add the button
    SET @xml = REPLACE(@xml, N'        <vb:VBButton Grid.Row="2" Grid.Column="1" VBContent="!AutoRenameVariables" Name="AutoRenameVariables_0"></vb:VBButton>
    </vb:VBGrid>', N'        <vb:VBButton Grid.Row="2" Grid.Column="1" VBContent="!AutoRenameVariables" Name="AutoRenameVariables_0"></vb:VBButton>
        <vb:VBButton Grid.Row="3" Grid.ColumnSpan="2" VBContent="!StartOPCUABrowser" Name="StartOPCUABrowser_0"></vb:VBButton>
    </vb:VBGrid>');
    UPDATE ACClassDesign SET XMLDesign = @xml, UpdateName = 'GIP', UpdateDate = GETDATE()
    WHERE ACClassID = @studioID AND ACIdentifier = 'TabConfigList';
END
GO

-- 4b) Avalonia (XMLDesign2)
DECLARE @xml2 nvarchar(max);
DECLARE @studioID2 uniqueidentifier = (SELECT ACClassID FROM ACClass WHERE ACIdentifier = 'BSOiPlusStudio');

SET @xml2 = (SELECT CAST(d.XMLDesign2 AS nvarchar(max)) FROM ACClassDesign d WHERE d.ACClassID = @studioID2 AND d.ACIdentifier = 'TabConfigList');
IF @xml2 IS NOT NULL AND @xml2 NOT LIKE '%StartOPCUABrowser%'
BEGIN
    SET @xml2 = REPLACE(@xml2, N'            <RowDefinition Height="*"></RowDefinition>
            <RowDefinition Height="40"></RowDefinition>
            <RowDefinition Height="40"></RowDefinition>
        </Grid.RowDefinitions>', N'            <RowDefinition Height="*"></RowDefinition>
            <RowDefinition Height="40"></RowDefinition>
            <RowDefinition Height="40"></RowDefinition>
            <RowDefinition Height="40"></RowDefinition>
        </Grid.RowDefinitions>');
    SET @xml2 = REPLACE(@xml2, N'        <vb:VBButton Grid.Row="2" Grid.Column="1" VBContent="!AutoRenameVariables" Name="AutoRenameVariables_0"></vb:VBButton>
    </vb:VBGrid>', N'        <vb:VBButton Grid.Row="2" Grid.Column="1" VBContent="!AutoRenameVariables" Name="AutoRenameVariables_0"></vb:VBButton>
        <vb:VBButton Grid.Row="3" Grid.ColumnSpan="2" VBContent="!StartOPCUABrowser" Name="StartOPCUABrowser_0"></vb:VBButton>
    </vb:VBGrid>');
    UPDATE ACClassDesign SET XMLDesign2 = @xml2, XMLDesign2UpdateDate = GETDATE(), UpdateName = 'GIP', UpdateDate = GETDATE()
    WHERE ACClassID = @studioID2 AND ACIdentifier = 'TabConfigList';
END
GO

PRINT 'BSOOPCUABrowser designs created / TabConfigList updated.';
