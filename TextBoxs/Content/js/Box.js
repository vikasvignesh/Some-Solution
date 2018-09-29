$(document).ready(function () {


    $("#addButton").click(function () {



        var newTextBoxDiv = $(document.createElement('div'))
             .attr("id", 'TextBoxDiv');

        newTextBoxDiv.after().html('<input type="text" name="textbox" id="textbox" value="" >');

        newTextBoxDiv.appendTo("#TextBoxesGroup");

    });
    $('.submit').click(function () {
        var formValues = $('#myid').serialize(); // gathers all of the form data from the elements with the class fieldname
        $.ajax({
            method: 'POST',
            url: '/Home/Index/', // this is for the jsfiddle test, change to your URL
            data: formValues, // put the variable here
            success: function (response) {
                //
            }
        });
    });


});

