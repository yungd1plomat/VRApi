using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRClient.Models;

namespace VRClient.Abstractions
{
    public interface IVRClient
    {
        /// <summary>
        /// Авторизация с указанным username и password
        /// </summary>
        /// <param name="username">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns></returns>
        Task AuthAsync(string username, string password);

        /// <summary>
        /// Получает информацию о текущем пользователе
        /// </summary>
        /// <returns></returns>
        Task<UserInfo> GetMeAsync();
    }
}
