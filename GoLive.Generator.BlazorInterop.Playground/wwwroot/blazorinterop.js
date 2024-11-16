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
    }
};