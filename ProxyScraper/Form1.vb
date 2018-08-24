Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Threading
Imports ProxyScraper.My.Resources


Public Class Form1
    Dim _sources As List(Of String) = New List(Of String)
    Dim _scraped As List(Of String) = New List(Of String)
    Dim _srcMax As Integer = 0
    Dim _thrdCnt As Integer = 0
    Dim _thrdMax As Integer = 50
    Dim _thrdDict As New Dictionary(Of String, Thread)()
    ReadOnly _listLock As Object = New Object()
    ReadOnly _LB2_Thread As Thread = New Thread(AddressOf Update_LB2)
    ReadOnly _PB_Thread As Thread = New Thread(AddressOf PB_Updater)
    Dim Running As Boolean = False

    Function Start() As Boolean
        If _sources.Any() Then
            MessageBox.Show("No sources loaded, using built-in ones.")
        Else
            LoadSrc()
        End If
        _srcMax = _sources.Count
        Running = True
        _LB2_Thread.IsBackground = True
        _LB2_Thread.Start()
        _PB_Thread.IsBackground = True
        _PB_Thread.Start()
        Dim thrdIndex = 1
        While _sources.Count > 0 and Running
            If _thrdCnt <= _thrdMax Then
                _thrdDict(thrdIndex.ToString) = New Thread(AddressOf ScrapeTask)
                _thrdDict(thrdIndex.ToString).IsBackground = True
                _thrdDict(thrdIndex.ToString).Start()
                _thrdCnt = _thrdCnt + 1
                thrdIndex += 1
            End If
        End While
        Thread.Sleep(20000)
        Return True

    End Function

    Sub ScrapeTask()
        If _sources.Count > 0 Then
            Dim toScrape As String
            SyncLock _listLock
                toScrape = _sources.Item(0)
                _sources.RemoveAt(0)
            End SyncLock
            _scraped.AddRange(ScrapeLink(toScrape).Distinct().ToList())
            _thrdCnt = _thrdCnt - 1
        End If
    End Sub

    Private Function ScrapeLink(link As String) As List(Of String)
        Dim proxies = New List(Of String)()
        Try 
            proxies = ExtractProx(New StreamReader(HttpWebRequestWrapper.CreateNew(link, 15000, UserAgent).GetResponse().GetResponseStream()).ReadToEnd())
        Catch E As Exception
            'handle exception
        End Try
        Return proxies 
    End Function

    Private Function ExtractProx(http As String) As List(Of String)
        Dim output = New List(Of String)

        For Each proxy As Match In Regex.Matches(http, "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+:[0-9]+")
            output.Add(proxy.ToString())
        Next
        Return output
    End Function

    Private Sub LoadSrc()
        Dim psrc As String = My.Resources.psrc
        _sources = psrc.Split("$").ToList()
        If Not _sources.Count > 0 Then
            LoadSrcWeb()
        End If
        _sources.RemoveAt(0)
        Update_LB1()
    End Sub

    Private Sub LoadSrcWeb()
        Dim client = New WebClient()
        Dim reader = New StreamReader(client.OpenRead(WebSrc))
        Dim temp As String = reader.ReadToEnd
        Dim tmpSrc As String() = temp.Split("$")
        _sources = tmpSrc.ToList()
        _sources.RemoveAt(0)
        Update_LB1()
    End Sub

    Sub SaveFile(tempL As List(Of String))
        If (tempL.Any()) Then
            Dim fs As New SaveFileDialog
            fs.RestoreDirectory = True
            fs.Filter = "txt files (*.txt)|*.txt"
            fs.FilterIndex = 1
            fs.ShowDialog()
            If Not (fs.FileName = Nothing) Then
                Using sw As New StreamWriter(fs.FileName)
                    For Each line As String In tempL
                        sw.WriteLine(line)
                    Next
                End Using
            End If
        Else
            MessageBox.Show("Nothing to save!")
        End If
    End Sub

    Function OpenFile() As List(Of String)
        Dim tempList = New List(Of String)
        Dim fo As New OpenFileDialog
        fo.RestoreDirectory = True
        fo.Filter = "txt files (*.txt)|*.txt"
        fo.FilterIndex = 1
        fo.ShowDialog()
        If Not (fo.FileName = Nothing) Then
            Using sr As New StreamReader(fo.FileName)
                Dim line as String
                Do
                    line = sr.ReadLine()
                    tempList.Add(line)
                Loop Until line is Nothing
            End Using
        End If

        return tempList
    End Function

    Private Sub Update_LB1()
        ListBox1.Invoke(Sub()
                            ListBox1.Text = _sources.ToString()
                            ListBox1.Update()
                        End Sub)
    End Sub

    Private Sub Update_LB2()
        While Running
                ListBox2.Invoke(Sub()
                            ListBox2.Text = _scraped.ToString()
                            ListBox2.Update()
                        End Sub)
            Thread.Sleep(100)
        End While
    End Sub

    Private Sub PB_Updater()
        While Running
            ProgressBar1.Invoke(Sub()
                             ProgressBar1.Value = Math.Round(_sources.Count/_srcMax)
                             ProgressBar1.Update()
                        End Sub)
            Thread.Sleep(100)
        End While
    End Sub

    'start
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Start()
    End Sub

    'stop
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Running = False
    End Sub

    'save
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        SaveFile(_scraped)
    End Sub

    'load file
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        _sources = OpenFile()
        Update_LB1()
    End Sub

    'load built-in
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        LoadSrc()
        Update_LB1()
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        _thrdMax = TrackBar1.Value
        NumericUpDown1.Value = TrackBar1.Value
    End Sub

    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged
        _thrdMax = NumericUpDown1.Value
        TrackBar1.Value = NumericUpDown1.Value
    End Sub
End Class
