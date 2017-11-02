<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.OutputBox = New System.Windows.Forms.RichTextBox()
        Me.AdbcdbDataSet = New BCAD2018.adbcdbDataSet()
        Me.TimeTableBindingSource = New System.Windows.Forms.BindingSource(Me.components)
        Me.TimeTableTableAdapter = New BCAD2018.adbcdbDataSetTableAdapters.TimeTableTableAdapter()
        Me.TableAdapterManager = New BCAD2018.adbcdbDataSetTableAdapters.TableAdapterManager()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.ProgressDisplay = New System.Windows.Forms.ProgressBar()
        Me.ModeLabel = New System.Windows.Forms.Label()
        CType(Me.AdbcdbDataSet, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.TimeTableBindingSource, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'OutputBox
        '
        Me.OutputBox.Location = New System.Drawing.Point(12, 12)
        Me.OutputBox.Name = "OutputBox"
        Me.OutputBox.Size = New System.Drawing.Size(762, 455)
        Me.OutputBox.TabIndex = 0
        Me.OutputBox.Text = ""
        '
        'AdbcdbDataSet
        '
        Me.AdbcdbDataSet.DataSetName = "adbcdbDataSet"
        Me.AdbcdbDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema
        '
        'TimeTableBindingSource
        '
        Me.TimeTableBindingSource.DataMember = "TimeTable"
        Me.TimeTableBindingSource.DataSource = Me.AdbcdbDataSet
        '
        'TimeTableTableAdapter
        '
        Me.TimeTableTableAdapter.ClearBeforeFill = True
        '
        'TableAdapterManager
        '
        Me.TableAdapterManager.BackupDataSetBeforeUpdate = False
        Me.TableAdapterManager.TimeTableTableAdapter = Me.TimeTableTableAdapter
        Me.TableAdapterManager.UpdateOrder = BCAD2018.adbcdbDataSetTableAdapters.TableAdapterManager.UpdateOrderOption.InsertUpdateDelete
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(805, 129)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 1
        Me.Button1.Text = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'ProgressDisplay
        '
        Me.ProgressDisplay.Location = New System.Drawing.Point(12, 473)
        Me.ProgressDisplay.Name = "ProgressDisplay"
        Me.ProgressDisplay.Size = New System.Drawing.Size(617, 35)
        Me.ProgressDisplay.Step = 1
        Me.ProgressDisplay.TabIndex = 2
        '
        'ModeLabel
        '
        Me.ModeLabel.AutoSize = True
        Me.ModeLabel.Location = New System.Drawing.Point(15, 521)
        Me.ModeLabel.Name = "ModeLabel"
        Me.ModeLabel.Size = New System.Drawing.Size(39, 13)
        Me.ModeLabel.TabIndex = 3
        Me.ModeLabel.Text = "Label1"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1076, 687)
        Me.Controls.Add(Me.ModeLabel)
        Me.Controls.Add(Me.ProgressDisplay)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.OutputBox)
        Me.Name = "Form1"
        Me.Text = "Form1"
        CType(Me.AdbcdbDataSet, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.TimeTableBindingSource, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents OutputBox As RichTextBox
    Friend WithEvents AdbcdbDataSet As adbcdbDataSet
    Friend WithEvents TimeTableBindingSource As BindingSource
    Friend WithEvents TimeTableTableAdapter As adbcdbDataSetTableAdapters.TimeTableTableAdapter
    Friend WithEvents TableAdapterManager As adbcdbDataSetTableAdapters.TableAdapterManager
    Friend WithEvents Button1 As Button
    Friend WithEvents ProgressDisplay As ProgressBar
    Friend WithEvents ModeLabel As Label
End Class
