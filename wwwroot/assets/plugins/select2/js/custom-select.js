$(document).ready(function () {
    // Basic Select2 Initialization
    $(".basic").select2({
        tags: true,
    });

    // Apply Select2 with Conditional Check
    var formSmall = $(".form-small");
    if (formSmall.length) {
        formSmall.select2({
            tags: true,
        }).data('select2').$container.addClass('form-control-sm');
    }

    // Initialize Other Select2 Elements
    $(".nested").select2({
        tags: true,
    });
    $(".tagging").select2({
        tags: true,
    });
    $(".disabled-results").select2();
    $(".placeholder").select2({
        placeholder: "Make a Selection",
        allowClear: true,
    });

    // Custom Template Selection
    function formatState(state) {
        if (!state.id) {
            return state.text;
        }
        var baseClass = "flaticon-";
        var $state = $(
            '<span><i class="' +
            baseClass +
            state.element.value.toLowerCase() +
            '"></i> ' +
            state.text +
            '</span>'
        );
        return $state;
    }

    $(".templating").select2({
        templateSelection: formatState,
    });

    // Example Single Selection
    $(".js-example-basic-single").select2();
});
