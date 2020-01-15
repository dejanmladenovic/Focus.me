var globalFile = null //for storin image generated insidea canvas
var globalFiles = Array();

function validateAndSubmitForm(formID) {
    const form = $(formID);
    if (form.length == 0) {
        console.log("form not found");
        return;
    }
    form.submit(function (e) {
        e.preventDefault(); 
        var form = $(formID);

        if (!form.valid()) {
            return
        }
        var url = form.attr('action');
        const formData = new FormData();

        var paramsArray = form.serializeArray();
        for (var i = 0; i < paramsArray.length; i++) {
            formData.append(paramsArray[i].name, paramsArray[i].value);
        }
        
        

        if (globalFile != null) {
            for (var i = 0; i < globalFiles.length; i++) {
                formData.append(globalFiles[i].name, globalFiles[i].value);
            }    
        }
        var redirectTo = form.attr("redirect");
        sendFormData();
        function sendFormData() {
            $.ajax({
                type: "POST",
                url: url,
                processData: false,
                contentType: false,
                data: formData, 
                success: function (data) {
                    if (data == "success") {
                        if (redirectTo == "refresh")
                            location.reload();
                        else
                            window.location.href = redirectTo;
                    }
                    else {
                        alert(data);
                    }
                }
            }); 
        }

    });
}