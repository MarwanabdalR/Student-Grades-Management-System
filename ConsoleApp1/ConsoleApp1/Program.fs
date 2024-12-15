module Program

open System
open System.Windows.Forms
open GUI
open Student_Sys

[<EntryPoint>]
let main _ =
    let (adminPanel, viewerPanel, loginPanel) = GUI.panels
    let (txtUsername, txtPassword, btnLogin) = GUI.loginElements
    let (txtID, txtName, txtGrades, btnAdd, btnEdit, btnRemove, btnShowAllStudents, btnReturnToLogin) = GUI.adminElements
    let (lstView, statisticsLabel) = GUI.viewerElements

    // Initialize Panels
    GUI.mainPanel.Controls.Add(loginPanel)

    // Add Logic to Buttons
    btnLogin.Click.Add(fun _ ->
        let username = txtUsername.Text
        let password = txtPassword.Text

        if username = "admin" && password = "admin" then
            Student_Sys.isAdmin <- true
            GUI.mainPanel.Controls.Clear()
            GUI.mainPanel.Controls.Add(adminPanel)
        elif Student_Sys.students |> List.exists (fun s -> s.Name = username && string s.ID = password) then
            Student_Sys.isAdmin <- false
            GUI.mainPanel.Controls.Clear()
            GUI.mainPanel.Controls.Add(viewerPanel)
        else
            MessageBox.Show("Invalid credentials!") |> ignore
    )

    btnAdd.Click.Add(fun _ ->
        if Student_Sys.isAdmin then
            try
                let id = int txtID.Text
                let name = txtName.Text
                let grades = txtGrades.Text.Split(',') |> Array.map int |> Array.toList
                LoStudent_Sysgic.students <- { ID = id; Name = name; Grades = grades } :: Student_Sys.students
                Student_Sys.saveData()
                MessageBox.Show("Student added successfully!") |> ignore
            with
            | _ -> MessageBox.Show("Invalid input!") |> ignore
    )

    btnReturnToLogin.Click.Add(fun _ ->
        GUI.mainPanel.Controls.Clear()
        GUI.mainPanel.Controls.Add(loginPanel)
    )

    Student_Sys.loadData()
    Application.Run(GUI.form)
    0
