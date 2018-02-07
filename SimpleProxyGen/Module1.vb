Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Module Module1

    Private _sources As List(Of String) = New List(Of String)
    Private _scraped As List(Of String) = New List(Of String)
    Private _working As List(Of String) = New List(Of String)
    Private _validSources As List(Of String) = New List(Of String)
    Private outFile As String = Environment.CurrentDirectory + "\proxies.txt"
    Private workingFile As String = Environment.CurrentDirectory + "\working.txt"

#Region "SCRAPER"
    'scrapes a given link for proxies
    Private Function ScrapeLink(link As String) As List(Of String)
        Dim proxies = New List(Of String)
        Try 'gets the entire web page as a string
            Dim r As HttpWebRequest = HttpWebRequest.Create(link)
            r.UserAgent =
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
            r.Timeout = 15000
            Using sr As New StreamReader(r.GetResponse().GetResponseStream())
                proxies = ExtractProx(sr.ReadToEnd())
            End Using
            r.Abort()
            Console.WriteLine(proxies.Count.ToString() + " || " + link)
            If proxies.Any() Then
                File.AppendAllLines(workingFile,{link})
                '_validSources.Add(link)
            End If
        Catch ex As Exception

        End Try
        'returns scraped result
        Return proxies
    End Function

    'finds all proxies in a given string, returns them as a List(Of String)
    Private Function ExtractProx(http As String) As List(Of String)
        Dim output = New List(Of String)

        For Each proxy As Match In Regex.Matches(http, "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+:[0-9]+")
            output.Add(proxy.ToString())
        Next
        Return output
    End Function

    'loads all source links from internal resources
    Private Sub LoadSrc()
        Dim psrc As String = My.Resources.psrc
        _sources = psrc.Split("$").ToList()
        If Not _sources.Count > 0 Then
            LoadSrcWeb()
        End If
        _sources.RemoveAt(0)
    End Sub

    'fallback method, will remove to keep my sources safe
    Private Sub LoadSrcWeb()
        Dim client = New WebClient()
        Dim reader = New StreamReader(client.OpenRead("https://pastebin.com/raw/KhiFJE4m"))
        Dim temp As String = reader.ReadToEnd
        Dim tmpSrc As String() = temp.Split("$")
        _sources = tmpSrc.ToList()
        _sources.RemoveAt(0)
    End Sub
#End Region

#Region "CHECKER"
    'test single proxy
    Function CheckProxy(proxy As String) As Boolean
        Try 'uses azenv.net proxy judge
            Dim r As HttpWebRequest = HttpWebRequest.Create("http://azenv.net")
            r.UserAgent =
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.2 Safari/537.36"
            r.Timeout = 3000
            r.ReadWriteTimeout = 3000
            r.Proxy = New WebProxy(proxy)
            Using sr As New StreamReader(r.GetResponse().GetResponseStream())
                If sr.ReadToEnd().Contains("HTTP_HOST = azenv.net") Then
                    r.Abort()
                    Return True
                End If
            End Using
            r.Abort()
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function
#End Region

#Region "MISC"
    Sub SaveFile(file As String, tempL As List(Of String))
        If (tempL.Any()) Then
            If Not (file = Nothing) Then
                Using sw As New StreamWriter(file)
                    For Each line As String In tempL
                        sw.WriteLine(line)
                    Next
                End Using
            End If
        Else
            MessageBox.Show("Nothing to save!")
        End If
    End Sub

    Function OpenFile(file As String) As List(Of String)
        Dim tempList = New List(Of String)
        If Not (file = Nothing) Then
            Using sr As New StreamReader(file)
                Dim line As String
                Do
                    line = sr.ReadLine()
                    tempList.Add(line)
                Loop Until line Is Nothing
            End Using
        End If
        Return tempList
    End Function
#End Region

    Sub Main(args As String())
        Console.WriteLine("Usage: SimpleProxyGen.exe -s [sourcefile] -o [outputfile] -w [workingsourcefile]" + vbNewLine + "If no params are provided, will use inbuilt sources, save proxies in local 'proxies.txt' file, and working sources in 'working.txt'")
       
        If args.Length >= 2 then
            If args.ElementAt(0).Equals("-s") Then
                _sources = OpenFile(args.ElementAt(1))
                Console.WriteLine("Using provided proxy sources - " + args.ElementAt(1))
            ElseIf args.ElementAt(0).Equals("-o") Then
                outFile = args.ElementAt(1)
            ElseIf args.ElementAt(0).Equals("-w") Then
                workingFile = args.ElementAt(1)
            End If
        Else 
            Console.WriteLine("Invalid or nonexistant arguments found, please check you started the program correctly!")
        End If

        If args.Length >= 4 Then 
            If args.ElementAt(2).Equals("-s") Then
                _sources = OpenFile(args.ElementAt(3))
                Console.WriteLine("Using provided proxy sources - " + args.ElementAt(3))
            ElseIf args.ElementAt(2).Equals("-o") Then
                outFile = args.ElementAt(3)
            ElseIf args.ElementAt(2).Equals("-w") Then
                workingFile = args.ElementAt(3)
            End If
        Else 
            Console.WriteLine("Invalid or nonexistant arguments found, please check you started the program correctly!")
        End If 

        If args.Length = 6 Then
            If args.ElementAt(4).Equals("-s") Then
                _sources = OpenFile(args.ElementAt(5))
                Console.WriteLine("Using provided proxy sources - " + args.ElementAt(5))
            ElseIf args.ElementAt(4).Equals("-o") Then
                outFile = args.ElementAt(5)
            ElseIf args.ElementAt(4).Equals("-w") Then
                workingFile = args.ElementAt(5)
            End If
        Else 
            Console.WriteLine("Invalid or nonexistant arguments found, please check you started the program correctly!")
        End If

        Console.WriteLine("Working proxies will be saved at: " + outFile)
        Console.WriteLine("Working sources will be saved at: " + workingFile + vbNewLine)

        LoadSrc()
        Console.WriteLine(_sources.Count.ToString() + " sources loaded" + vbNewLine)
        For Each link As String In _sources
            'File.AppendAllLines()
            _scraped.AddRange(ScrapeLink(link))
        Next
        _sources.Clear()
        For Each proxy As String In _scraped
            If CheckProxy(proxy) Then
                File.AppendAllLines(outFile,{proxy})
                '_working.Add(proxy)
                Console.WriteLine(proxy)
            End If
        Next
        _scraped.Clear()
        'SaveFile(outFile, _working)
        'SaveFile(workingFile, _validSources)
        _validSources.Clear()
        _working.Clear()
        Console.WriteLine("Thanks for using Eric's Basic ProxyGen!")
    End Sub

End Module
