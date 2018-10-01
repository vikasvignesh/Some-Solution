$(document).ready(function () {


    $("#addButton").click(function () {



        var newTextBoxDiv = $(document.createElement('div'))
             .attr("id", 'TextBoxDiv');

        newTextBoxDiv.after().html('<br><input type="text" name="id" id="textbox" value="" class="control"><br><br><input type="text" name="id" id="textbox" value="" class="control"><br><br><input type="text" name="id" id="textbox" value="" class="control"><br><br><input type="text" name="id" id="textbox" value="" class="control"><br><br><input type="text" name="id" id="textbox" value="" class="control"><br><br><input type="text" name="id" id="textbox" value="" class="control"><br><br><input type="text" name="id" id="textbox" value="" class="control"><br><br><input type="text" name="id" id="textbox" value="" class="control"><br><br><input type="text" name="id" id="textbox" value="" class="control"><br><br>');

        newTextBoxDiv.appendTo("#TextBoxesGroup3");

        var newTextBoxDiv = $(document.createElement('div'))
             .attr("id", 'TextBoxDiv');

        newTextBoxDiv.after().html('<br><input type="text" name="time" id="textbox" value="" class="control" ><br><br><input type="text" name="time" id="textbox" value="" class="control" ><br><br><input type="text" name="time" id="textbox" value="" class="control" ><br><br><input type="text" name="time" id="textbox" value="" class="control" ><br><br><input type="text" name="time" id="textbox" value="" class="control" ><br><br><input type="text" name="time" id="textbox" value="" class="control" ><br><br><input type="text" name="time" id="textbox" value="" class="control" ><br><br><input type="text" name="time" id="textbox" value="" class="control" ><br><br><input type="text" name="time" id="textbox" value="" class="control" >');

        newTextBoxDiv.appendTo("#TextBoxesGroup4");


    });
    $('.submit').click(function () {
        //var error = document.getElementById('er').innerHTML;
        var formValues = $('#myid').serialize();
        $.ajax({
            method: 'POST',
            url: '/Document/AssignAsanaTask/',
            data: formValues, 
            success: function (response) {
                //
            }
        });
    });
    function errors() {
        alert("Given id Wrong");
    }

});
