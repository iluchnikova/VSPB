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
    public partial class Form3 : Form
    {
        string userLogin;// Переменная для передачи имени входящего в программу из формы авторизации

        public Form3(string userLogin)
        {
            InitializeComponent();
            this.userLogin = userLogin;
        }

        private async void Form3_Load(object sender, EventArgs e)
        {
            // Подключаемся к БД
            SqlConnection connectionWorkers = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Samsung\source\repos\VSPB\VSPB\Workers.mdf;Integrated Security=True");
            await connectionWorkers.OpenAsync();

            // Настройка автозаполнения поля "Кому" с помощью свойства AutoCompleteCustomSource:
            SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM Workers", connectionWorkers);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            AutoCompleteStringCollection collectionDepartment = new AutoCompleteStringCollection();
            foreach (DataRow row in dt.Rows)
            {
                collectionDepartment.Add(row["Отдел"].ToString());
            }
            textBox1.AutoCompleteCustomSource = collectionDepartment;
            textBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Поставить задачу
            // Проверка на наличие незаполненных полей
            if (!string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(textBox2.Text) && !string.IsNullOrEmpty(richTextBox1.Text)
                && !string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox2.Text) && !string.IsNullOrWhiteSpace(richTextBox1.Text))
            {
                // Проверка на соответствие названия отдела наименованиям в БД Workers
                SqlConnection connectionWorkers = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Samsung\source\repos\VSPB\VSPB\Workers.mdf;Integrated Security=True");
                await connectionWorkers.OpenAsync();
                SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM Workers", connectionWorkers);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                if (dt.Select("Отдел Like '%"+ textBox1.Text+"%'").Length >=1)
                {
                    // Подключаемся к БД
                    SqlConnection connectionTasks = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Samsung\source\repos\VSPB\VSPB\Tasks.mdf;Integrated Security=True");
                    await connectionTasks.OpenAsync();
                    // Добавляем в БД полученные значения
                    SqlCommand command = new SqlCommand("INSERT INTO Tasks (Логин_отправителя, Получатель, Тема, Задача, Решение, Статус)VALUES(@Логин_отправителя, @Получатель, @Тема, @Задача, @Решение, @Статус)", connectionTasks);
                    command.Parameters.AddWithValue("Логин_отправителя", userLogin);
                    command.Parameters.AddWithValue("Получатель", textBox1.Text);
                    command.Parameters.AddWithValue("Тема", textBox2.Text);
                    command.Parameters.AddWithValue("Задача", richTextBox1.Text);
                    command.Parameters.AddWithValue("Решение", DBNull.Value);
                    command.Parameters.AddWithValue("Статус", "new");

                    await command.ExecuteNonQueryAsync();

                    this.Hide();// Скрывается форма
                }
                else
                {
                    MessageBox.Show("Проверьте правильность заполнения поля 'Кому'.");
                }
            }
            else
            {
                MessageBox.Show("Все поля должны быть заполнены");
            }

        }
    }
}
