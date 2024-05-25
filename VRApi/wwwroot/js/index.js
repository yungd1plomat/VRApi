$(document).ready(function () {
    let table = new DataTable('#myTable');

    function validatePasswords() {
        var password = $('#password').val();
        var confirmPassword = $('#confirmPassword').val();

        if (password !== confirmPassword) {
            $('#error-message').fadeIn();;
        } else {
            $('#error-message').fadeOut();;
        }
    }

    $('#confirmPassword').on('input', validatePasswords);

    $('#addForm').on('submit', function (e) {
        if ($('#error-message').is(':visible')) {
            e.preventDefault(); // Не отправлять форму, если есть ошибка
        }
    });
})
