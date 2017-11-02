Imports System.DirectoryServices.AccountManagement
Imports System.IO
Imports AshbyTools

Public Class Form1
    Dim myEventLog As EventLog
    Public Delegate Sub LogDelegate(ByVal status As String)
    Dim bromcomReader As SoapReader.TPReadOnlyDataServiceSoapClient = New SoapReader.TPReadOnlyDataServiceSoapClient("TPReadOnlyDataServiceSoap")
    Dim currday As String = GetDay(Now)
    Dim automatic As Boolean = False

    Dim empties As String = ""
    Dim newbies As String = ""

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetupLogging()
        Dim arguments As System.Collections.ObjectModel.ReadOnlyCollection(Of String) = My.Application.CommandLineArgs
        For Each arg In arguments
            If arg.Equals("/?") Then
                OutputBox.AppendText(String.Format("{0} /? = This help.{1}", Application.ProductName, vbCrLf))
                OutputBox.AppendText(String.Format("{0} /a = Automatically sync current staff and students and then exit.{1}", Application.ProductName, vbCrLf))
            End If
            If arg.Equals("/a") Then
                automatic = True
                myEventLog.WriteEntry("Startup Automatic Mode")
            End If
        Next
        If Not automatic Then
            myEventLog.WriteEntry("Startup Manual Mode")
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SyncAllStudents()
    End Sub

#Region "Logging"
    Private Sub SetupLogging()
        Try
            If Not EventLog.SourceExists(My.Application.Info.ProductName) Then
                EventLog.CreateEventSource(My.Application.Info.ProductName, "BCAD")
            End If

            myEventLog = New EventLog With {
                .Source = My.Application.Info.ProductName,
                .Log = "BCAD",
                .EnableRaisingEvents = True
            }
            AddHandler myEventLog.EntryWritten, AddressOf Log_Informer
        Catch ex As Exception
            MsgBox("Cannot create eventlog")
        End Try

    End Sub

    Private Sub Log_Informer(ByVal sender As Object, ByVal e As EntryWrittenEventArgs)
        LogInfo(String.Format("{0} {1}{2}{3}{4}", e.Entry.TimeGenerated.ToShortDateString, e.Entry.TimeGenerated.ToShortTimeString, vbTab, e.Entry.Message, vbCrLf))
    End Sub

    Private Sub LogInfo(ByVal s As String)
        If Me.InvokeRequired Then
            OutputBox.Invoke(New LogDelegate(AddressOf LogInfo), s)
        Else
            OutputBox.AppendText(s)
        End If
    End Sub


#End Region

#Region "timetable"

    Private Function GetDay(ByVal checkDate As Date) As String
        Dim outline As String = "not a school day"
        Dim calent As DataTable
        Dim calendar As DataTable
        Dim filterString As String = String.Format("CONVERT(date, '{0}') = CONVERT(date, StartDate)", checkDate.ToString("yyyy-MM-dd'T'HH:mm:ss"))
        calendar = getTable("Calendars", filterString)
        calent = getTable("CalendarModels", "")
        Dim query As IEnumerable = From link In calent
                                   Join cal In calendar
                    On link.Field(Of Integer)("CalendarModelID") Equals cal.Field(Of Integer)("CalendarModelID")
                                   Select New With {.calid = cal.Field(Of Integer)("CalendarModelID"),
                                     .calname = link.Field(Of String)("CalendarModelName")
                                    }

        For Each thingy In query
            Dim dayString As String = thingy.calname
            If dayString.StartsWith("Day_") Then
                Dim day As String = dayString.Split(" ")(0).Replace("Day_", "")
                Dim period As String = dayString.Split(" ")(1)
                outline = String.Format("{0}", day)
            End If
        Next
        Return outline
    End Function

    Public Function GetStudentTimetable(ByVal user As String, ByRef testdate As Date, ByRef days As Integer) As DataTable
        Dim tt As New DataTable
        Dim studT As DataTable
        Dim timeT As DataTable
        Dim studTT As DataTable
        Dim startDate As Date = testdate.Date
        Dim endDate As Date = startDate.AddDays(days)

        Dim studFilterS As String = String.Format("StudentID like '{0}'", user)
        studT = getTable("Students", studFilterS)

        Dim studid As Integer = studT.Rows(0).Field(Of Integer)("StudentID")
        Dim timeFilterS As String = String.Format("'{0}' < PeriodStartDate AND '{1}' > PeriodEndDate", startDate.ToString("yyyy-MM-dd'T'HH:mm:ss"), endDate.ToString("yyyy-MM-dd'T'HH:mm:ss"))
        Dim studTTFilterS As String = String.Format("StudentID like '{0}' and '{2}' > PeriodStartDate and '{1}' < PeriodEndDate", studid, startDate.ToString("yyyy-MM-dd'T'HH:mm:ss"), endDate.ToString("yyyy-MM-dd'T'HH:mm:ss"))
        timeT = getTable("Timetable", timeFilterS)
        studTT = getTable("StudentTimeTables", studTTFilterS)

        Dim ttquery = From link In studTT.AsEnumerable
                      Join theTimeTable In timeT
                      On link.Field(Of Date)("PeriodStartDate") Equals theTimeTable.Field(Of Date)("PeriodStartDate") And link.Field(Of Integer)("ClassID") Equals theTimeTable.Field(Of Integer?)("ClassID")
                      Order By theTimeTable.Field(Of Date)("PeriodStartDate")
                      Select New With {
                                       .periodStart = theTimeTable.Field(Of Date)("PeriodStartDate").ToString("yyyy-MM-dd'T'HH:mm:ss"),
                                       .period = link.Field(Of String)("PeriodName"),
                                       .location = link.Field(Of String)("LocationName"),
                                       .Class = theTimeTable.Field(Of String)("ClassName"),
                                       .dayOfWeek = theTimeTable.Field(Of String)("DayOfWeek"),
                                       .ttDay = theTimeTable.Field(Of Integer)("TimetableDay")}

        tt.Columns.Add("Day", Type.GetType("System.String"))
        tt.Columns.Add("Period 1", Type.GetType("System.String"))
        tt.Columns.Add("Period 2", Type.GetType("System.String"))
        tt.Columns.Add("Reg", Type.GetType("System.String"))
        tt.Columns.Add("Break", Type.GetType("System.String"))
        tt.Columns.Add("Period 3", Type.GetType("System.String"))
        tt.Columns.Add("Period 4", Type.GetType("System.String"))
        tt.Columns.Add("Period 5", Type.GetType("System.String"))
        tt.Columns.Add("Period 6", Type.GetType("System.String"))
        tt.Columns.Add("PreSchool", Type.GetType("System.String"))
        tt.Columns.Add("AfterSchool", Type.GetType("System.String"))

        Dim ttr As DataRow = tt.NewRow()
        Dim lastday As String = "99"
        For Each item In ttquery
            If lastday.Equals("99") Then
                lastday = item.dayOfWeek
                ttr("Day") = item.dayOfWeek
            End If
            If (Not item.dayOfWeek.Equals(lastday)) Then
                tt.Rows.Add(ttr)
                lastday = item.dayOfWeek
                ttr = tt.NewRow()
            Else
                ttr("Day") = item.dayOfWeek
                Select Case item.period
                    Case "1"
                        ttr("Period 1") = item.Class & " (" & item.location & ")"
                    Case "2"
                        ttr("Period 2") = item.Class & " (" & item.location & ")"
                    Case "3"
                        ttr("Period 3") = item.Class & " (" & item.location & ")"
                    Case "4"
                        ttr("Period 4") = item.Class & " (" & item.location & ")"
                    Case "5"
                        ttr("Period 5") = item.Class & " (" & item.location & ")"
                    Case "6"
                        ttr("Period 6") = item.Class & " (" & item.location & ")"
                    Case "AM"
                        ttr("Reg") = item.Class & " (" & item.location & ")"
                    Case "BR1"
                        ttr("Break") = item.Class & " (" & item.location & ")"
                    Case "PS"
                        ttr("PreSchool") = item.Class & " (" & item.location & ")"
                    Case "7"
                        ttr("AfterSchool") = item.Class & " (" & item.location & ")"
                End Select
            End If
        Next
        tt.Rows.Add(ttr)
        Return tt
    End Function

    Public Function GetStaffTimetable(ByVal user As String, ByRef testdate As Date, ByRef days As Integer) As DataTable
        Dim startDate As Date = testdate.Date
        Dim endDate As Date = startDate.AddDays(days)
        Dim staffT As DataTable
        Dim timeT As DataTable
        Dim staffFilterS As String = String.Format("StaffID Like '{0}'", user)
        Dim timeFilterS As String = String.Format("'{0}' < PeriodStartDate AND '{1}' > PeriodEndDate", startDate.ToString("yyyy-MM-dd'T'HH:mm:ss"), endDate.ToString("yyyy-MM-dd'T'HH:mm:ss"))

        staffT = getTable("Staff", staffFilterS)
        timeT = getTable("Timetable", timeFilterS)

        Dim query = From link In timeT.AsEnumerable()
                    Join staffM In staffT.AsEnumerable()
            On link.Field(Of Nullable(Of Integer))("StaffID") Equals staffM.Field(Of Integer?)("StaffID")
                    Order By link.Field(Of String)("WeekDayPeriod") Ascending
                    Where link.Field(Of System.DateTime)("PeriodEndDate").DayOfYear = startDate.DayOfYear
                    Select New With {.Staff = staffM.Field(Of String)("PreferredFullName"),
                             .ClassName = link.Field(Of String)("ClassName"),
                             .Start = link.Field(Of String)("WeekDayPeriod"),
                             .End = link.Field(Of System.DateTime)("PeriodEndDate"),
                             .Period = link.Field(Of String)("PeriodName"),
                             .Location = If(link.Field(Of String)("LocationName"), link.Field(Of String)("StaffTimetableCodeDescription"))}

        Dim att As New DataTable
        att.Columns.Add("PreSchool", Type.GetType("System.String"))
        att.Columns.Add("Period 1", Type.GetType("System.String"))
        att.Columns.Add("Period 2", Type.GetType("System.String"))
        att.Columns.Add("Reg", Type.GetType("System.String"))
        att.Columns.Add("Break", Type.GetType("System.String"))
        att.Columns.Add("Period 3", Type.GetType("System.String"))
        att.Columns.Add("Period 4", Type.GetType("System.String"))
        att.Columns.Add("Period 5", Type.GetType("System.String"))
        att.Columns.Add("Period 6", Type.GetType("System.String"))
        att.Columns.Add("AfterSchool", Type.GetType("System.String"))

        Dim ttr As DataRow = att.NewRow()
        For Each item In query
            Select Case item.Period
                Case "1"
                    ttr("Period 1") = item.ClassName & " (" & item.Location & ")"
                Case "2"
                    ttr("Period 2") = item.ClassName & " (" & item.Location & ")"
                Case "3"
                    ttr("Period 3") = item.ClassName & " (" & item.Location & ")"
                Case "4"
                    ttr("Period 4") = item.ClassName & " (" & item.Location & ")"
                Case "5"
                    ttr("Period 5") = item.ClassName & " (" & item.Location & ")"
                Case "6"
                    ttr("Period 6") = item.ClassName & " (" & item.Location & ")"
                Case "AM"
                    ttr("Reg") = item.ClassName & " (" & item.Location & ")"
                Case "BR1"
                    ttr("Break") = item.ClassName & " (" & item.Location & ")"
                Case "PS"
                    ttr("PreSchool") = item.ClassName & " (" & item.Location & ")"
                Case "7"
                    ttr("AfterSchool") = item.ClassName & " (" & item.Location & ")"
            End Select

        Next

        att.Rows.Add(ttr)

        Return att
    End Function

    Private Sub WriteTimetableToDB(ByRef ttr As DataRow, ByVal userID As String)
        Dim exists As Integer = TimeTableTableAdapter.UserExists(userID)
        If exists > 0 Then
            TimeTableTableAdapter.DeleteUser(userID)
            TimeTableTableAdapter.InsertQuery(userID, ttr.Field(Of String)("Period 1"), ttr.Field(Of String)("Period 2"), ttr.Field(Of String)("Period 3"), ttr.Field(Of String)("Period 4"), ttr.Field(Of String)("Period 5"), ttr.Field(Of String)("Period 6"),
                                              ttr.Field(Of String)("Reg"), ttr.Field(Of String)("Break"), ttr.Field(Of String)("PreSchool"), ttr.Field(Of String)("AfterSchool"), currday)
        Else
            TimeTableTableAdapter.InsertQuery(userID, ttr.Field(Of String)("Period 1"), ttr.Field(Of String)("Period 2"), ttr.Field(Of String)("Period 3"), ttr.Field(Of String)("Period 4"), ttr.Field(Of String)("Period 5"), ttr.Field(Of String)("Period 6"),
                                              ttr.Field(Of String)("Reg"), ttr.Field(Of String)("Break"), ttr.Field(Of String)("PreSchool"), ttr.Field(Of String)("AfterSchool"), currday)
        End If
    End Sub

    Private Sub TimeTableBindingNavigatorSaveItem_Click(sender As Object, e As EventArgs)
        Me.Validate()
        Me.TimeTableBindingSource.EndEdit()
        Me.TableAdapterManager.UpdateAll(Me.AdbcdbDataSet)

    End Sub

#End Region

#Region "Sync"
    Private Sub SyncAllStudents()
        Dim est As Integer = 0
        Dim runLen As Integer = 0
        Dim completed As Integer = 1
        Dim managedGroups As List(Of String) = getManagedGroupNames(tutorGroupsCTX)
        managedGroups.AddRange(getManagedGroupNames(subjectGroupsCTX))
        Dim studentTable As DataTable = getTable("Students", currentStudentsFilter)
        myEventLog.WriteEntry(String.Format("Sync Students - {0} records", studentTable.Rows.Count))
        Dim cnt As Integer = 0
        Dim sTime As Date = Now
        Dim cTime As Date = Now
        ProgressDisplay.Maximum = studentTable.Rows.Count
        For Each studentRow As DataRow In studentTable.Rows
            cTime = Now
            runLen = (cTime - sTime).TotalMilliseconds
            completed = ProgressDisplay.Maximum - (ProgressDisplay.Maximum - ProgressDisplay.Value) + 1
            est = (((runLen / completed) * (ProgressDisplay.Maximum - ProgressDisplay.Value)) / 1000) / 60
            cnt = cnt + 1
            OutputBox.AppendText(String.Format("Cnt = {0}{1}", cnt, vbCrLf))
            SyncStudent(studentRow, managedGroups)
            Application.DoEvents()
            ProgressDisplay.PerformStep()
            ModeLabel.Text = String.Format("{0}/{1} (est {2} Minutes - running {3} Minutes)", ProgressDisplay.Value, ProgressDisplay.Maximum, est, (runLen / 1000) / 60)
        Next
        SendMessage("Groups Sync Results", String.Format("Empties = {1}{0}New = {2}", vbCrLf, empties, newbies))
    End Sub

    Private Sub SyncStudent(ByRef sr As DataRow, ByRef managedGroups As List(Of String))
        Dim tt As DataTable = GetStudentTimetable(sr.Field(Of Integer)("StudentID").ToString, Now, 1)
        WriteTimetableToDB(tt.Rows(0), sr.Field(Of Integer)("StudentID".ToString))
        SyncGroups(sr, managedGroups)
    End Sub


    Private Sub SyncGroups(ByRef sr As DataRow, ByRef managedGroups As List(Of String))
        Try
            Dim tutorGroup As String = getStudentTutorGroup(sr.Field(Of Integer)("StudentID".ToString))
            Dim studentGroups As List(Of String) = getStudentClassList(sr.Field(Of Integer)("StudentID".ToString))
            Dim user As UserPrincipal = getUserPrincipalByID(userCTX, sr.Field(Of Integer)("StudentID".ToString))
            Dim currentGrps As PrincipalSearchResult(Of Principal) = user.GetGroups()
            Dim currentGroups As New List(Of String)
            Dim emptyGroups As New List(Of String)
            Dim newGroups As New List(Of String)

            For Each grp In currentGrps
                If StartsWithNumber(grp.Name) Then
                    currentGroups.Add(grp.Name)
                End If
            Next

            'Remove from old groups
            For Each grp As String In currentGroups
                If Not studentGroups.Contains(grp) Then
                    removeUserFromGroup(groupsCTX, sr.Field(Of Integer)("StudentID".ToString), grp)
                    If getGroupMembers(grp).Count = 0 Then
                        emptyGroups.Add(grp)
                    End If
                End If
            Next

            'Add to new groups
            For Each grp As String In studentGroups
                If Not currentGroups.Contains(grp) Then
                    If Not managedGroups.Contains(grp) Then
                        createGroup(subjectGroupsCTX, grp)
                        managedGroups.Add(grp)
                        newGroups.Add(grp)
                    End If
                    addUserToGroup(subjectGroupsCTX, sr.Field(Of Integer)("StudentID".ToString), grp)
                End If
            Next

            For Each item As String In emptyGroups
                empties = item & " "
            Next
            For Each item As String In newGroups
                newbies = item & " "
            Next
        Catch ex As Exception
            SendMessage("Sync Failed", String.Format("Sync failed for  {0}", sr.Field(Of Integer)("StudentID".ToString)))
        End Try


        OutputBox.AppendText("Done")
    End Sub

#End Region

#Region "utils"
    Private Function IsWeekday(ByVal s As String) As Boolean
        If {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"}.Contains(s) Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub SendMessage(ByVal subject As String, ByVal message As String)
        Sendmail.EmailUser = "p-page"
        Sendmail.EmailPass = "Tafidm01"
        Sendmail.EmailServer = "mail.ashbyschool.org.uk"
        Dim msg As New eMailMessage With {
            .mailfrom = "itsupport@ashbyschool.org.uk",
            .mailto = "itsupport@ashbyschool.org.uk",
            .subject = subject,
            .body = message,
            .isBodyHTML = False
        }
        Sendmail.sendmail(msg)
    End Sub

    Private Function StartsWithNumber(ByVal s As String) As Boolean
        Dim sValArray As Char() = s.ToCharArray
        If IsNumeric(sValArray(0)) Then
            Return True
        Else
            Return False
        End If
    End Function


#End Region
End Class
