# Тестирование страницы регистрации/авторизации
Скриншот базы данных пользователей (SQL-скрипт содержится в составе проекта):
![изображение](https://github.com/user-attachments/assets/821d23b5-42ed-4913-9a12-bcb5d37eab1b)
Обозреватель тестов:
![изображение](https://github.com/user-attachments/assets/4861560a-9e32-449a-8896-45c33d2d295b)
![изображение](https://github.com/user-attachments/assets/3dc4e310-6deb-4b71-99a5-495b0bf5a7cc)
Видно, что в первом случае тест вывел ошибку, однако после изменения строчки `Assert.IsTrue(page.Auth("user", "12345qwe"));` на `Assert.IsFalse(page.Auth("user", "12345qwe"));` она была исправлена.
