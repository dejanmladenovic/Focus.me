var globalFile = {name: null, value: null} //for storin image generated insidea canvas

function validateAndSubmitForm(formID, imagesWidth) {
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
        //var fileInputs = form.find("input[type='file']");
        
        var hasFiles = false;

        if (globalFile.name != null) {
            formData.append(globalFile.name, globalFile.value);
        }
        /*
        for (var i = 0; i < fileInputs.length; i++) {
            if (fileInputs[i].files.length > 0) {
                hasFiles = true;
                if (fileInputs[i].files[0].type == "image/jpeg") {
                    const file = fileInputs[i].files[0];
                    if (file.size > 3 * 1024 * 1024) {
                        alert("Previse veliki dokument");
                        return;
                    }
                    const fieldName = fileInputs[i].name;
                    if (globalFile.name != null && globalFile.name != fieldName) {
                        formData.append(globalFile.name, globalFile.value);
                    }
                    new Compressor(file, {
                        quality: 0.7,
                        convertSize: 0,
                        width: imagesWidth,
                        success(result) {
                            formData.append(fieldName, result
                            sendFormData();
                        }
                    })
                }
                else {
                    alert("tip dokumenta nije podrzan");
                    return;
                }
            }
        }*/
        sendFormData();
        
        function sendFormData() {
            $.ajax({
                type: "POST",
                url: url,
                processData: false,
                contentType: false,
                data: formData, 
                success: function (data) {
                    alert(data); 
                }
            }); 
        }

    });
}