window.blazorInterop = {
    showModal: function (dialogId) {
        $(dialogId).modal('show');
        return true;
    },
    hideModal: function (dialogId) {
        $(dialogId).modal('hide');

        return true;
    },
    setPageTitle: function(title) {
        document.title = title;
    },
    datePicker: function(id) {
        $(id).datepicker({
            dateFormat: "yy-mm-dd"
        });
    },
    addNumbers: function(x, y) {
        return x + y;
    },
    formatDate: function(day, month, year) {
        return new Date(year, month - 1, day);
    },
    greetUser: function(name, isFormal) {
        return (isFormal ? "Dear " : "Hello ") + name;
    },
};