using IO.Swagger.Models;
using IO.Swagger.Models.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using XSystem.Security.Cryptography;

namespace IO.Swagger.Services
{
    public class MongoDBServices
    {
        private readonly IMongoCollection<Users> _userscollection;
        private string claveSecreta;

        public MongoDBServices(IOptions<MongoDBSettings> mongoDBSettings, IConfiguration config)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _userscollection = database.GetCollection<Users>(mongoDBSettings.Value.CollectionName);
            claveSecreta = config.GetValue<string>("ApiSettings:Secreta");
        }


        public async Task<ResponseLogin> Login(RequestLogin login)
        {
            var passwordEncriptado = obtenermd5(login.Password);

            //var usuario = _userscollection.Find(
            //    u => u.User.ToLower() == login.User.ToLower() 
            //    && u.Password == passwordEncriptado).FirstOrDefault();

            var usuario = _userscollection
                .Find(u => u.User.ToLower() == login.User.ToLower())
                .FirstOrDefault();

            if (usuario == null)
            {
                return null;

                //return new ResponseLogin()
                //{
                //    Token = "",
                //    Users = null
                //};
            }

            var userPassEncriptado = obtenermd5(usuario.Password);

            if (!userPassEncriptado.Equals(passwordEncriptado))
            {
                return null;
            }

            var manejadorToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(claveSecreta);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.User),
                    new Claim(ClaimTypes.Role, usuario.Channel)

                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)

            };

            var token = manejadorToken.CreateToken(tokenDescriptor);
            ResponseLogin usuariologinrespuesta = new ResponseLogin()
            {
                Token = manejadorToken.WriteToken(token),
                Users = usuario
            };
            return usuariologinrespuesta;
        }

        public static string obtenermd5(string valor)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(valor);
            data = x.ComputeHash(data);
            string resp = "";
            for (int i = 0; i < data.Length; i++)
                resp += data[i].ToString("x2").ToLower();
            return resp;
            
        }
    }
}
