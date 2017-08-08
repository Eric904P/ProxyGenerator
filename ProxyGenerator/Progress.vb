Public Class Progress
    WithEvents _checker As Checker = New Checker()
    WithEvents _scraper As Scraper = New Scraper()
    Public Running As Boolean = False

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Show()
        Update()
    End Sub

    Private Sub _StepUI()
        Label1.Text = _scraper.ReturnScrapedCount().ToString() + "/" + _scraper.ReturnSourceCount().ToString()
        Label2.Text = _checker.ReturnCheckedCount().ToString() + "/" + _checker.ReturnScrapedTotal().ToString()
        ProgressBar1.Value = _scraper.ReturnPercent()
        ProgressBar2.Value = _checker.ReturnPercent()
        Label5.Text = _scraper.ReturnThreadCount().ToString()
        Label6.Text = _checker.ReturnThreadCount().ToString()
        Update()
    End Sub

    Private Sub CheckEventHandle(sender As Object) Handles _checker.StepUi
        _StepUI()
    End Sub

    Private Sub ScrapeEventHandle(sender As Object) Handles _scraper.StepUi
        _StepUI()
    End Sub
End Class