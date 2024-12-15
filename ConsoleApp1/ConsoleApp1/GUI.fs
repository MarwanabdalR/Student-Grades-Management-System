module GUI

open System
open System.Windows.Forms
open System.Drawing

// Main form
let form = new Form(Text = "Student Grades Management System", Width = 900, Height = 700, BackColor = Color.Beige, Font = new Font("Arial", 10.0F, FontStyle.Bold))

let mainPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.LavenderBlush, Padding = Padding(20))
form.Controls.Add(mainPanel)

// Admin Panel
let adminPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.MistyRose, Padding = Padding(20))

let lblID = new Label(Text = "Student ID", ForeColor = Color.DarkSlateGray, Font = new Font("Arial", 12.0F, FontStyle.Bold), AutoSize = true)
let txtID = new TextBox(PlaceholderText = "Enter Student ID", Width = 300)

let lblName = new Label(Text = "Student Name", ForeColor = Color.DarkSlateGray, Font = new Font("Arial", 12.0F, FontStyle.Bold), AutoSize = true)
let txtName = new TextBox(PlaceholderText = "Enter Student Name", Width = 300)

let lblGrades = new Label(Text = "Grades (comma-separated)", ForeColor = Color.DarkSlateGray, Font = new Font("Arial", 12.0F, FontStyle.Bold), AutoSize = true)
let txtGrades = new TextBox(PlaceholderText = "Enter Grades", Width = 300)

let btnAdd = new Button(Text = "Add Student", Width = 250, BackColor = Color.SeaGreen, ForeColor = Color.White)
let btnEdit = new Button(Text = "Edit Student", Width = 250, BackColor = Color.SteelBlue, ForeColor = Color.White)
let btnRemove = new Button(Text = "Remove Student", Width = 250, BackColor = Color.IndianRed, ForeColor = Color.White)
let btnShowAllStudents = new Button(Text = "Show All Students", Width = 250, BackColor = Color.DarkGoldenrod, ForeColor = Color.White)
let btnReturnToLogin = new Button(Text = "Return to Login", Width = 250, BackColor = Color.SlateGray, ForeColor = Color.White)

adminPanel.Controls.AddRange([| lblID; txtID; lblName; txtName; lblGrades; txtGrades; btnAdd; btnEdit; btnRemove; btnShowAllStudents; btnReturnToLogin |])

// Viewer Panel
let viewerPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.Honeydew, Padding = Padding(20))
let lstView = new ListBox(Width = 500, Height = 300, BackColor = Color.White)
let statisticsLabel = new Label(Text = "Statistics: (Will update dynamically)", Font = new Font("Arial", 12.0F, FontStyle.Italic), ForeColor = Color.DarkSlateGray)

viewerPanel.Controls.AddRange([| lstView; statisticsLabel |])

// Login Panel
let loginPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.LightBlue, Padding = Padding(22))
let txtUsername = new TextBox(PlaceholderText = "Username", Width = 300)
let txtPassword = new TextBox(PlaceholderText = "Password", PasswordChar = '*', Width = 300)
let btnLogin = new Button(Text = "Login", Height = 40, BackColor = Color.MediumSeaGreen, ForeColor = Color.White)

loginPanel.Controls.AddRange([| txtUsername; txtPassword; btnLogin |])

// Exported GUI elements
let panels = (adminPanel, viewerPanel, loginPanel)
let loginElements = (txtUsername, txtPassword, btnLogin)
let adminElements = (txtID, txtName, txtGrades, btnAdd, btnEdit, btnRemove, btnShowAllStudents, btnReturnToLogin)
let viewerElements = (lstView, statisticsLabel)
