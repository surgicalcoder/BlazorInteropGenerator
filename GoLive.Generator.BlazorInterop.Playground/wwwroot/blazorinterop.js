window.blazorInterop = {
    /**
     * Shows a modal dialog by its CSS selector.
     * @param {string} dialogId - The CSS selector for the dialog element
     * @returns {boolean} True if the modal was shown
     */
    showModal: function (dialogId) {
        $(dialogId).modal('show');
        return true;
    },
    /**
     * Hides a modal dialog by its CSS selector.
     * @param {string} dialogId - The CSS selector for the dialog element
     * @returns {boolean} True if the modal was hidden
     */
    hideModal: function (dialogId) {
        $(dialogId).modal('hide');
        return true;
    },
    /**
     * Sets the browser page title.
     * @param {string} title - The new page title
     */
    setPageTitle: function(title) {
        document.title = title;
    },
    /**
     * Initializes a datepicker on the given element.
     * @param {string} id - The CSS selector for the input element
     */
    datePicker: function(id) {
        $(id).datepicker({
            dateFormat: "yy-mm-dd"
        });
    },
    /**
     * Adds two numbers together.
     * @param {number} x - First number
     * @param {number} y - Second number
     * @returns {number} The sum of x and y
     */
    addNumbers: function(x, y) {
        return x + y;
    },
    /**
     * Formats a date from day, month, year components.
     * @param {number} day - Day of the month
     * @param {number} month - Month (1-12)
     * @param {number} year - Full year
     * @returns {Date} The constructed Date object
     */
    formatDate: function(day, month, year) {
        return new Date(year, month - 1, day);
    },
    /**
     * Greets a user with a formal or informal greeting.
     * @param {string} name - The user's name
     * @param {boolean} isFormal - Whether to use a formal greeting
     * @returns {string} The greeting message
     */
    greetUser: function(name, isFormal) {
        return (isFormal ? "Dear " : "Hello ") + name;
    },
};
