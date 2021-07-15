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
    public partial class Form4 : Form
    {
        int ID;
        string userLogin;
        string userDepartment;
        string status;
        SqlConnection connectionWorkers;
        SqlConnection connectionTasks;

        public Form4(int ID, string userLogin, string userDepartment)
        {
            InitializeComponent();

            this.ID = ID;
            this.userDepartment = userDepartment;
            this.userLogin = userLogin;
        }

        private async void Form4_Load(object sender, EventArgs e)
        {
            connectionWorkers = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Samsung\source\repos\VSPB\VSPB\Workers.mdf;Integrated Security=True");
            await connectionWorkers.OpenAsync();

            connectionTasks = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Samsung\source\repos\VSPB\VSPB\Tasks.mdf;Integrated Security=True");
            await connectionTasks.OpenAsync();

            SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Tasks WHERE Id = '" + ID.ToString() + "'", connectionTasks);
            // Создаем объект Dataset
            DataSet ds = new DataSet();
            // Заполняем Dataset
            adapter.Fill(ds);
            string loginOfSender = ds.Tables[0].Rows[0][1].ToString();

            //Преобразуем логин отправителя в ФИО (Должность, Отдел)
            SqlDataAdapter adapter1 = new SqlDataAdapter("SELECT * FROM Workers WHERE Логин = '" + loginOfSender + "'", connectionWorkers);
            // Создаем объект Dataset
            DataSet ds1 = new DataSet();
            // Заполняем Dataset
            adapter1.Fill(ds1);
            string taskSender = ds1.Tables[0].Rows[0][2].ToString() + " " + ds1.Tables[0].Rows[0][3].ToString() + " "
                + ds1.Tables[0].Rows[0][4].ToString() + " (" + ds1.Tables[0].Rows[0][6].ToString() + ", "
                + ds1.Tables[0].Rows[0][5].ToString() + ")";

            //Заполняем форму:
            textBox1.Text = taskSender;
            textBox2.Text = ds.Tables[0].Rows[0][2].ToString();
            textBox3.Text = ds.Tables[0].Rows[0][3].ToString();
            richTextBox1.Text = ds.Tables[0].Rows[0][4].ToString();
            richTextBox2.Text = ds.Tables[0].Rows[0][5].ToString();

            //Устанавливаем имя/наличие кнопки в зависимости от статуса задачи
            button1.Visible = false;
            status = ds.Tables[0].Rows[0][6].ToString();
            if (status == "new" && textBox2.Text == userDepartment)
            {
                button1.Visible = true;
                button1.Text = "Ответить";
            }
            else if (status == "not_compl" && loginOfSender == userLogin)
            {
                button1.Visible = true;
                button1.Text = "Задача решена";
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            connectionTasks = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Samsung\source\repos\VSPB\VSPB\Tasks.mdf;Integrated Security=True");
            await connectionTasks.OpenAsync();

            SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Tasks WHERE Id = '" + ID + "'", connectionTasks);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            status = ds.Tables[0].Rows[0][6].ToString();

            if (status == "new")
            {
                if (string.IsNullOrEmpty(richTextBox2.Text) || string.IsNullOrWhiteSpace(richTextBox2.Text))
                    MessageBox.Show("Поле 'Решение' должно быть заполнено.");
                else
                {
                    status = "not_compl";

                    SqlCommand command = new SqlCommand("UPDATE Tasks SET Решение = @Решение ,Статус = @Статус WHERE Id = @Id", connectionTasks);

                    command.Parameters.AddWithValue("Решение", richTextBox2.Text);
                    command.Parameters.AddWithValue("Статус", status);
                    command.Parameters.AddWithValue("Id", ID);

                    await command.ExecuteNonQueryAsync();

                    this.Hide();
                }
            }
            else if (status == "not_compl")
            {
                status = "completed";

                SqlCommand command = new SqlCommand("UPDATE Tasks SET Статус = @Статус WHERE Id = @Id", connectionTasks);

                command.Parameters.AddWithValue("Статус", status);
                command.Parameters.AddWithValue("Id", ID);

                await command.ExecuteNonQueryAsync();

                this.Hide();
            }
        }
    }
}
