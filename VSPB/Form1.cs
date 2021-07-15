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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox2.PasswordChar = '*';//Маскировка пароля
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Подключение к БД с логинами и паролями пользователей
            SqlConnection connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Samsung\source\repos\VSPB\VSPB\Users.mdf;Integrated Security=True");
            await connection.OpenAsync();
            try
            {
                // Проверка на совпадение введенных данных с БД (значение = количество совпадений)
                SqlDataAdapter sda = new SqlDataAdapter("SELECT Count (*) FROM Users WHERE Пользователь = '" + textBox1.Text + "' and Пароль = '" + textBox2.Text + "'", connection);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                if (dt.Rows.Count.ToString() == "1") //если найдено только 1 совпадение
                {
                    Form2 f = new Form2(this.textBox1.Text);// Передаем в основную программу логин пользователя
                    this.Hide();// Скрывается форма авторизации
                    f.Show();// Открывается окно основной программы
                }
                else
                {
                    MessageBox.Show("Введите логин и пароль"); // Сообщение об ошибке
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
