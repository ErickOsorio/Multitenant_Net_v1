using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using TentiaCloud.UtilitiesAPI.Entities;
using TentiaCloud.UtilitiesAPI.Models.Db;
using TentiaCloud.UtilitiesAPI.Repository.IRepository;

namespace TentiaCloud.UtilitiesAPI.Repository
{
    public class UsersRepository : IUsersRepository
    {
        private readonly GlobalDBContext _context;
        private readonly Tenant _connection;
        private readonly IConfiguration _configuration;

        public UsersRepository(GlobalDBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            if (httpContextAccessor.HttpContext != null)
            {
                _connection = (Tenant)httpContextAccessor.HttpContext.Items["TENANT"];
            }
        }

        public ICollection<Users> GetAllUsers()
        {
            // Obtenemos los datos del usuario
            MySqlConnection conn = new(_connection.CadenaConexion);
            MySqlCommand Cmd = new();
            MySqlDataReader reader = null;
            Cmd.CommandType = CommandType.Text;
            Cmd.CommandText = "SELECT idadmin, apellidos, nombreusuario, idperfil, idempleado, idSuc FROM administracion_usuarios";
            Cmd.Connection = conn;

            conn.Open();
            reader = Cmd.ExecuteReader();

            Users user = null;
            List<Users> listUser = new();
            while (reader.Read())
            {
                user = new Users
                {
                    idadmin = Convert.ToInt32(reader.GetValue(0)),
                    apellidos = reader.GetValue(1).ToString(),
                    nombreusuario = reader.GetValue(2).ToString(),
                    idperfil = Convert.ToInt32(reader.GetValue(3)),
                    idempleado = Convert.ToInt32(reader.GetValue(4)),
                    sucursal = reader.GetValue(5).ToString()
                };
                listUser.Add(user);
            }
            conn.Close();
            return listUser;
        }

        public ICollection<Users> GetUserById(int Id)
        {

            // Obtenemos los datos del usuario
            MySqlConnection conn = new(_connection.CadenaConexion);
            MySqlDataReader reader = null;
            MySqlCommand Cmd = new MySqlCommand();
            Cmd.CommandType = CommandType.Text;
            Cmd.CommandText = "SELECT au.idadmin, au.apellidos, au.nombreusuario, au.idperfil, au.idempleado, ms.nombre as sucursal FROM administracion_usuarios au, mrp_sucursal ms where au.idSuc = ms.idSuc and au.idadmin = '" + Id + "'";
            Cmd.Connection = conn;


            conn.Open();
            reader = Cmd.ExecuteReader();

            Users user = null;
            List<Users> listUser = new();
            while (reader.Read())
            {
                user = new Users
                {
                    idadmin = Convert.ToInt32(reader.GetValue(0)),
                    apellidos = reader.GetValue(1).ToString(),
                    nombreusuario = reader.GetValue(2).ToString(),
                    idperfil = Convert.ToInt32(reader.GetValue(3)),
                    idempleado = Convert.ToInt32(reader.GetValue(4)),
                    sucursal = reader.GetValue(5).ToString()
                };

                listUser.Add(user);
            }
            conn.Close();

            return listUser;
        }
    }
}
