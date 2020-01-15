//import Compressor from "./compressor/compressor.js";


$(document).ready(function () {
    $("#post_image_blob").on("change", function (e) {
        if ($(this)[0].files[0].type != "image/jpeg") {
            alert("Tip dokumenta je nepodrzan.");
            return;
        }
        $(".file-field-container").height(100);
        
        var canvas = document.getElementById('image-canvas');
        var ctx = canvas.getContext('2d');

        var reader = new FileReader();
        reader.onload = function (readerEvent) {
            var image = new Image();
            image.onload = function () {
                var canvasWidth = 900;
                var canvasHeight = 900;
                var imgWidth = image.width;
                var newImgW = 0;
                var newImgH = 0;
                var imgHeight = image.height;
                var clippingX = 0;
                var clippingY = 0;

                $("#image-canvas").prop("height", canvasHeight);
                $("#image-canvas").prop("width", canvasWidth);

                if (imgWidth > imgHeight) {
                    newImgW = canvasWidth
                    newImgH = imgHeight / imgWidth * canvasHeight;
                    clippingY = (canvasHeight - newImgH) / 2;
                }
                else {
                    newImgH = canvasHeight
                    newImgW = imgWidth / imgHeight * canvasWidth;
                    clippingX = (canvasWidth - newImgW) / 2;
                }
                ctx.fillStyle = '#f2f3ec';
                ctx.fillRect(0, 0, canvas.width, canvas.height);
                ctx.drawImage(image, clippingX, clippingY, newImgW, newImgH);
                var generatedImg = dataURLtoBlob(canvas.toDataURL("image/jpeg").replace("image/png", "image/octet-stream"));

                new Compressor(generatedImg, {
                    quality: 0.7,
                    success(result) {
                        globalFiles.push({ name: $("#post_image_blob")[0].name, value: result })
                        new Compressor(generatedImg, {
                            quality: 0.7,
                            width: 400,
                            success(smallResult) {
                                globalFiles.push({ name: $("#post_image_blob")[0].name + "_thumbnail", value: smallResult })
                                $(".upload-info-container button[type='submit']").prop("disabled", false);
                            }    
                        })
                        globalFile = true;
                    }
                })
                $(".upload-info-container").addClass("expand-height");
            }
            image.src = readerEvent.target.result;
        }
        reader.readAsDataURL($(this)[0].files[0]);
        
    })
    $(".upload-page").height($(document).height() - $(".navbar").height());
})


function dataURLtoBlob(dataurl) {
    var arr = dataurl.split(','), mime = arr[0].match(/:(.*?);/)[1],
        bstr = atob(arr[1]), n = bstr.length, u8arr = new Uint8Array(n);
    while (n--) {
        u8arr[n] = bstr.charCodeAt(n);
    }
    return new Blob([u8arr], { type: mime });
}