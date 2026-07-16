(function ($) {
    if (!$ || !$.validator) return;

    $.validator.addMethod('integeronly', function (value, element) {
        return this.optional(element) || /^\d+$/.test(value);
    }, 'Enter a whole number only.');

    $.validator.addMethod('decimalonly', function (value, element) {
        return this.optional(element) || /^\d+(\.\d{1,2})?$/.test(value);
    }, 'Enter a valid number with up to 2 decimal places.');

    $.validator.addMethod('phoneonly', function (value, element) {
        return this.optional(element) || /^[\d+\-() ]+$/.test(value);
    }, 'Phone may only contain numbers and + - ( ) spaces.');

    if (!$.validator.unobtrusive) return;

    $.validator.unobtrusive.adapters.add('integeronly', function (options) {
        options.rules.integeronly = true;
        options.messages.integeronly = options.message;
    });

    $.validator.unobtrusive.adapters.add('decimalonly', function (options) {
        options.rules.decimalonly = true;
        options.messages.decimalonly = options.message;
    });

    $.validator.unobtrusive.adapters.add('phoneonly', function (options) {
        options.rules.phoneonly = true;
        options.messages.phoneonly = options.message;
    });
})(window.jQuery);
