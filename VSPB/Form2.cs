using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace VSPB
{
    public partial class Form2 : Form
    {
        string userLogin;// Переменная для передачи имени входящего в программу из формы авторизации
        string userDepartment;
        SqlConnection connectionWorkers;
        SqlConnection connectionTasks;


        public Form2(string userLogin)
        {
            InitializeComponent();

            this.userLogin = userLogin;
        }

        //Асинхронное программирование позволяет избежать появления узких мест производительности 
        //и увеличить общую скорость реагирования приложения
        private void Form2_Load(object sender, EventArgs e)
        {
            обновитьToolStripMenuItem_Click(sender, e);
        }

        private async void обновитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Очищаем данные таблиц
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();

            // Подключаемся к БД
            connectionWorkers = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Samsung\source\repos\VSPB\VSPB\Workers.mdf;Integrated Security=True");

            await connectionWorkers.OpenAsync();

            // Вывод в форму ФИО вошедшего:
            // Выбираем строку, совпадающую с введенным логином
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Workers WHERE Логин = '" + userLogin + "'", connectionWorkers);
            // Создаем объект Dataset
            DataSet ds = new DataSet();
            // Заполняем Dataset
            adapter.Fill(ds);
            // Присваиваем переменной name значение ФИО из полученной таблицы
            string fullUserName = ds.Tables[0].Rows[0][2].ToString() + " " + ds.Tables[0].Rows[0][3].ToString() + " " + ds.Tables[0].Rows[0][4].ToString();
            userDepartment = ds.Tables[0].Rows[0][6].ToString();
            label1.Text = fullUserName + " (" + userDepartment + ")";

            // Выводим содержимое БД Tasks
            connectionTasks = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Samsung\source\repos\VSPB\VSPB\Tasks.mdf;Integrated Security=True");

            await connectionTasks.OpenAsync();

            // Получим БД в табличном представлении
            SqlDataReader sqlReader = null;

            var commandTask = connectionTasks.CreateCommand();
            commandTask.CommandText = "SELECT * FROM Tasks WHERE Получатель = @userDepartment AND Статус in ('new','not_compl')";
            commandTask.Parameters.AddWithValue("userDepartment", userDepartment);
            commandTask.Connection = connectionTasks;
            List<string[]> tasks = new List<string[]>();

            var commandMyTask = connectionTasks.CreateCommand();
            commandMyTask.CommandText = "SELECT * FROM Tasks WHERE Логин_отправителя = @userLogin AND Статус in ('new','not_compl')";
            commandMyTask.Parameters.AddWithValue("userLogin", userLogin);
            commandMyTask.Connection = connectionTasks;
            List<string[]> myTasks = new List<string[]>();

            var commandArchiv = connectionTasks.CreateCommand();
            commandArchiv.CommandText = "SELECT * FROM Tasks WHERE Статус = 'completed' AND Логин_отправителя = @userLogin " +
                "OR Статус = 'completed' AND Получатель = @userDepartment";
            commandArchiv.Parameters.AddWithValue("userLogin", userLogin);
            commandArchiv.Parameters.AddWithValue("userDepartment", userDepartment);
            commandArchiv.Connection = connectionTasks;
            List<string[]> archiv = new List<string[]>();

            try
            {
                //Вывод информации во вкладку "Задачи"
                sqlReader = await commandTask.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    // Преобразуем логин отправителя в ФИО (Отдел)
                    // При нахождении совпадения логина_отправителя с логином из БД Workers в переменную NameAndDepartment записываем ФИО и отдел
                    string login = Convert.ToString(sqlReader["Логин_отправителя"]);
                    SqlDataAdapter adapter1 = new SqlDataAdapter("SELECT * FROM Workers WHERE Логин = '" + login + "'", connectionWorkers);
                    DataSet ds1 = new DataSet();
                    adapter1.Fill(ds1);
                    string NameAndDepartmentOfSender = ds1.Tables[0].Rows[0][2].ToString() + " " + ds1.Tables[0].Rows[0][3].ToString().Substring(0, 1) + ". "
                        + ds1.Tables[0].Rows[0][4].ToString().Substring(0, 1) + ". (" + ds1.Tables[0].Rows[0][6].ToString() + ")";

                    tasks.Add(new string[3]);

                    tasks[tasks.Count - 1][0] = sqlReader[0].ToString();// ID
                    tasks[tasks.Count - 1][1] = sqlReader[3].ToString();// Тема
                    tasks[tasks.Count - 1][2] = NameAndDepartmentOfSender;// Отправитель                   
                }
                // Вывод в форму
                foreach (string[] task in tasks)
                {
                    dataGridView1.Rows.Add(task);
                }
                sqlReader.Close();
                //Выделение жирным шрифтом задач без ответа
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    // Получаем значение ID из dataGridView1
                    int IdFromDataGridViev = Convert.ToInt32(row.Cells[0].Value);
                    // Находим ID в Taska. Статус записываем в строку statusOfTask

                    var command = connectionTasks.CreateCommand();
                    command.CommandText = "SELECT * FROM Tasks WHERE Id = @IdFromDataGridViev";
                    command.Parameters.AddWithValue("IdFromDataGridViev", IdFromDataGridViev);
                    command.Connection = connectionTasks;

                    string statusOfTask = null;

                    using (sqlReader = await command.ExecuteReaderAsync())
                    {
                        if (sqlReader.Read())
                        {
                            statusOfTask = sqlReader.GetString(6);
                        }
                    }

                    // Если статус = new, шрифт меняется на жирный
                    if (statusOfTask == "new")
                    {
                        row.DefaultCellStyle.Font = new Font(dataGridView1.DefaultCellStyle.Font, FontStyle.Bold);
                    }
                }
                sqlReader.Close();

                // Вывод информации во вкладку "Мои задачи"
                sqlReader = await commandMyTask.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    myTasks.Add(new string[3]);

                    myTasks[myTasks.Count - 1][0] = sqlReader[0].ToString();// ID
                    myTasks[myTasks.Count - 1][1] = sqlReader[3].ToString();// Тема
                    myTasks[myTasks.Count - 1][2] = sqlReader[2].ToString();// Получатель                    
                }
                //Вывод в форму
                foreach (string[] task in myTasks)
                {
                    dataGridView2.Rows.Add(task);
                }
                sqlReader.Close();
                //Выделение жирным шрифтом задач на которые получено решение
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    // Получаем значение ID из dataGridView2
                    int IdFromDataGridViev = Convert.ToInt32(row.Cells[0].Value);
                    // Находим ID в Taska. Статус записываем в строку statusOfTask
                    var command = connectionTasks.CreateCommand();
                    command.CommandText = "SELECT * FROM Tasks WHERE Id = @IdFromDataGridViev";
                    command.Parameters.AddWithValue("IdFromDataGridViev", IdFromDataGridViev);
                    command.Connection = connectionTasks;

                    string statusOfTask = null;

                    using (sqlReader = await command.ExecuteReaderAsync())
                    {
                        if (sqlReader.Read())
                        {
                            statusOfTask = sqlReader.GetString(6);
                        }
                    }

                    // Если статус = not_compl, шрифт меняется на жирный
                    if (statusOfTask == "not_compl")
                    {
                        row.DefaultCellStyle.Font = new Font(dataGridView2.DefaultCellStyle.Font, FontStyle.Bold);
                    }
                }
                sqlReader.Close();

                //Вывод информации во вкладку "Архив"
                sqlReader = await commandArchiv.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    archiv.Add(new string[3]);

                    archiv[archiv.Count - 1][0] = sqlReader[0].ToString();// ID
                    archiv[archiv.Count - 1][1] = sqlReader[3].ToString();// Тема
                    archiv[archiv.Count - 1][2] = sqlReader[2].ToString();// Получатель                                       
                }
                //Вывод в форму
                foreach (string[] task in archiv)
                {
                    dataGridView3.Rows.Add(task);
                }
                sqlReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (connectionWorkers != null && connectionWorkers.State != ConnectionState.Closed)
                connectionWorkers.Close();

            if (connectionTasks != null && connectionTasks.State != ConnectionState.Closed)
                connectionTasks.Close();

            Application.Exit();
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Создание новой задачи
            Form3 form = new Form3(userLogin);
            form.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Открытие задачи

            // Определяем ID задачи в зависимости от открытой вкладки и выбранной строки
            int ID = 0;
            if (tabControl1.SelectedTab.Name == "tabPage1")
                ID = Convert.ToInt32(dataGridView1.CurrentRow.Cells[0].Value);
            if (tabControl1.SelectedTab.Name == "tabPage2")
                ID = Convert.ToInt32(dataGridView2.CurrentRow.Cells[0].Value);
            if (tabControl1.SelectedTab.Name == "tabPage3")
                ID = Convert.ToInt32(dataGridView3.CurrentRow.Cells[0].Value);

            Form4 form = new Form4(ID, userLogin, userDepartment);
            form.Show();
        }
    }
}
