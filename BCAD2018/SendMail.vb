Imports System.Net.Mail
Module Sendmail
    Private _emailPass As String
    Private _emailServer As String
    Private _emailUser As String

    Public Property EmailPass As String
        Get
            Return _emailPass
        End Get
        Set(value As String)
            _emailPass = value
        End Set
    End Property
    Public Property EmailServer As String
        Get
            Return _emailServer
        End Get
        Set(value As String)
            _emailServer = value
        End Set
    End Property

    Public Property EmailUser As String
        Get
            Return _emailUser
        End Get
        Set(value As String)
            _emailUser = value
        End Set
    End Property


    Public Sub sendmail(msg As eMailMessage)
        Try
            Dim Smtp_Server As New SmtpClient
            Dim e_mail As New MailMessage()
            Smtp_Server.UseDefaultCredentials = False
            Smtp_Server.Credentials = New Net.NetworkCredential(emailUser, emailPass)
            Smtp_Server.Port = 25
            Smtp_Server.EnableSsl = False
            Smtp_Server.Host = emailServer

            e_mail = New MailMessage()
            e_mail.From = New MailAddress(msg.mailfrom)
            e_mail.To.Add(msg.mailto)
            e_mail.Subject = msg.subject
            e_mail.IsBodyHtml = msg.isBodyHTML
            e_mail.Body = msg.body
            Smtp_Server.Send(e_mail)

        Catch error_t As Exception

        End Try
    End Sub

End Module

Public Class eMailMessage
    Public mailfrom As String
    Public mailto As String
    Public subject As String
    Public isBodyHTML As Boolean = False
    Public body As String

    Public Sub New()
        mailfrom = "NoReply@ashbyschool.org.uk"
        mailto = "itsupport@ashbyschool.org.uk"
        subject = "Automailed message"
        isBodyHTML = False
        body = "blank"
    End Sub

    Public Sub New(ByVal mailFrom As String, mailTo As String, subject As String, body As String)
        With Me
            mailFrom = mailFrom
            mailTo = mailTo
            subject = subject
            body = body
            isBodyHTML = False
        End With
    End Sub

End Class
