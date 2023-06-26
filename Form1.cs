using CsvHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CorrigeProtocolo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int posicaoCalc = 0;

        private void btnConectar_Click(object sender, EventArgs e)
        {
            try
            {
                string connectionString = "Data Source=" + editHost.Text + ";Initial Catalog=" + editDb.Text + "; User ID=Engegraph;Password=DevEngegraph";
                string query = "";
                string csvFilePath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "resultado.csv";


                for (int i = 0; i <= 5; i++)
                {

                    if (i == 0)
                    {
                        query = "select 1 as TIPO, n.CHAVE_NASCIMENTOS, n.DATA_LAVRATURA, null as PROTOCOLO from NASCIMENTOS n where n.DATA_LAVRATURA >= '01.01.2021'";

                    }
                    else if (i == 1)
                    {
                        query = "SELECT 2 as TIPO, c.CHAVE_CASAMENTOS, c.DATA_LAVRATURA, null as PROTOCOLO FROM CASAMENTOS c where c.DATA_LAVRATURA >= '01.01.2021'";
                    }
                    else if (i == 3)
                    {
                        query = "select 3 as TIPO, o.CHAVE_OBITOS, o.DATA_LAVRATURA, null as PROTOCOLO from OBITOS o where o.DATA_LAVRATURA >= '01.01.2021'";
                    }
                    else if (i == 4)
                    {
                        query = "select 4 as TIPO, nt.CHAVE_NATIMORTOS, nt.DATA_LAVRATURA, null as PROTOCOLO  from NATIMORTOS nt where nt.DATA_LAVRATURA >= '01.01.2021'";
                    }
                    else if (i == 5)
                    {
                        query = "select 5 as TIPO, le.CHAVE_LIVRO_E, le.DATA_LAVRATURA, null as PROTOCOLO from LIVRO_E le where le.DATA_LAVRATURA >= '01.01.2021'";
                    }

                    lblTeste.Text = i.ToString();
                    
                    // Consulta dos dados
                    DataTable dataTable = GetData(connectionString, query);

                    // Exportação para arquivo CSV
                    AppendToCsv(dataTable, csvFilePath, i);

                    //Exportar arquivo 2 ordenado
                    OrdenarCsv(csvFilePath);


                }
            }
            catch (SqlException erro)
            {

                MessageBox.Show("Não foi possível realizar conexão: "+ erro.Message);
            }

            try
            {
                string connectionString = "Data Source=" + editHost.Text + ";Initial Catalog=" + editDb.Text + "; User ID=Engegraph;Password=DevEngegraph";
                string csvFilePath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "resultado.sql";

                //Aplica o update no arquivo
                DataTableCompleto(System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "resultado.csv");

                UpdateEmBanco(connectionString);

                lblTeste.Text = "Arquivo gerado com sucesso!";
            }
            catch (SqlException erro)
            {

                MessageBox.Show("Não foi possível realizar conexão: " + erro.Message);
            }


        }

        static DataTable GetData(string connectionString, string query)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    dataTable.Load(reader);
                }
            }

            return dataTable;
        }

        public void AppendToCsv(DataTable dataTable, string csvFilePath, int primeiro)
        {
            using (var stream = File.Open(csvFilePath, FileMode.Append, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
            {                
                
                // Escrever os registros
                foreach (DataRow row in dataTable.Rows)
                {
                    posicaoCalc++;

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if (dataTable.Columns[i].DataType == typeof(DateTime))
                        {
                            DateTime dateValue = (DateTime)row[i];
                            string formattedDate = dateValue.ToString("yyyy/MM/dd"); // Change the format as desired
                            csv.WriteField(formattedDate);
                        }
                        else
                        {
                            csv.WriteField(row[i]);
                        }
                    }
                   
                    csv.NextRecord();
                }
            }
        }

        public void OrdenarCsv(string csvFilePath)
        {
            //leitura do arquivo
            string[] lines = System.IO.File.ReadAllLines(csvFilePath);
            
            //query dentro do arquivo usando o Split 
            IEnumerable<string> query =
                from line in lines
                let x = line.Split(',')
                orderby x[2]
                select x[0] + "," + x[1] + "," + x[2] + "," + x[3];// +","+ x[4];
            
            System.IO.File.WriteAllLines(System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "resultado.csv", query.ToArray());

        }


        public void DataTableCompleto(string csvFilePath)
        {
            // Ler todas as linhas do arquivo
            List<string> lines = new List<string>(File.ReadAllLines(csvFilePath));

            //Apontador de protocolo
            int secProtocolo = 1;

            for (int i = 0; i < lines.Count; i++)
            {
                // Dividir a linha em colunas usando a vírgula como separador
                string[] columns = lines[i].Split(',');

                if (0 < columns.Length)
                {
                    if(columns[0].ToString() == "1")
                    {
                        columns[3] = "update nascimentos set numero_protocolo = " + secProtocolo + ", DATA_PROTOCOLO = DATA_LAVRATURA where chave_nascimentos = " + columns[1].ToString();
                        secProtocolo++;
                    }
                    else if (columns[0].ToString() == "2")
                    {
                        columns[3] = "update casamentos set numero_protocolo = " + secProtocolo + ", DATA_PROTOCOLO = DATA_LAVRATURA where chave_casamentos = " + columns[1].ToString();
                        secProtocolo++;
                    }
                    else if (columns[0].ToString() == "3")
                    {
                        columns[3] = "update obitos set numero_protocolo = " + secProtocolo + ", DATA_PROTOCOLO = DATA_LAVRATURA where chave_obitos = " + columns[1].ToString();
                        secProtocolo++;
                    }
                    else if (columns[0].ToString() == "4")
                    {
                        columns[3] = "update natimortos set numero_protocolo = " + secProtocolo + ", DATA_PROTOCOLO = DATA_LAVRATURA where chave_natimortos = " + columns[1].ToString();
                        secProtocolo++;
                    }
                    else if (columns[0].ToString() == "5")
                    {
                        columns[3] = "update livro_e set numero_protocolo = " + secProtocolo + ", DATA_PROTOCOLO = DATA_LAVRATURA where chave_livro_e = " + columns[1].ToString();
                        secProtocolo++;
                    }

                }

                string updatedLine = string.Join(";", columns[3]);
                lines[i] = updatedLine;
                
            }

            // Gravar as linhas atualizadas no mesmo arquivo
            File.WriteAllLines(System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "resultado.sql", lines);            
        }

        public void UpdateEmBanco(string connect)
        {

            // Ler o conteúdo do arquivo .sql
            string sqlScript = File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "resultado.sql");

            // Conectar ao banco de dados
            using (SqlConnection connection = new SqlConnection(connect))
            {
                try
                {
                    connection.Open();

                    // Criar um objeto SqlCommand para executar o script SQL
                    using (SqlCommand command = new SqlCommand(sqlScript, connection))
                    {
                        // Executar o script SQL
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Script executado com sucesso!");

                    //para o caso do banco não conter livro protocolo
                    if (MessageBox.Show("Deseja incluir o livro a partir do dia 01.01.2021?", "Corrigir Protocolo RCPN", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        string striptLivroProtocolo = "INSERT INTO LIVRO_PROTOCOLO (CHAVE_LIVRO_PROTOCOLO, DATADIA, LIVRO, NUMERO_LIVRO_INI, NUMERO_LIVRO_FIM, NUMERO_PAGINA_INI, NUMERO_PAGINA_FIM, DATA_SISTEMA) VALUES((select COALESCE(MAX(chave_livro_protocolo) + 1, 0+1) FROM LIVRO_PROTOCOLO), '01.01.2021', '1', 1.00, 1.00, 1.00, 200.00, '01.01.2024')";

                        using (SqlCommand command = new SqlCommand(striptLivroProtocolo, connection))
                        {
                            // Executar o script SQL
                            command.ExecuteNonQuery();

                            MessageBox.Show("Livro inserido com sucesso!");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Ocorreu um erro: " + ex.Message);
                }
            }
        }

        private void sobreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            form.ShowDialog();
        }
    }
}
