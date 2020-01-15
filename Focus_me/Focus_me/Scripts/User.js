var userName = null;
var pagination = null;

$(document).ready(function () {
    userName = $("input#user_name").val();
    $("#follow-unfollow").on("click", function () {
        followUnfollow(this);
    })
    $(".user-stats-btn").on("click", function () {
        showFollowers(this);
    })
    loadUserImages();
})

function followUnfollow(element) {
    var btn = $(element);

    var following_user_name = btn.attr("following_user_name");
    var following_full_name = btn.attr("following_full_name");
    var following_image_blob = btn.attr("following_image_blob");

    if (following_user_name == null || following_full_name == null) {
        alert("user creditenals error. " + following_user_name + following_full_name);
    }

    var formData = new FormData();
    formData.append("following_user_name", following_user_name);
    formData.append("following_full_name", following_full_name);
    if (following_image_blob != null) {
        formData.append("following_image_blob", following_image_blob);
    }

    var target = btn.attr("target");

    $.ajax({
        type: "POST",
        url: "/users/"+target,
        processData: false,
        contentType: false,
        data: formData,
        success: function (data) {
            if (data == "success") {
                if (target == "FollowUser") {
                    $("#number-of-followers").html(parseInt($("#number-of-followers").html()) + 1);
                    btn.attr("target", "UnfollowUser");
                    btn.removeClass("btn-primary");
                    btn.addClass("btn-secondary");
                    btn.html("Otprati");
                }
                else {
                    $("#number-of-followers").html(parseInt($("#number-of-followers").html()) - 1);
                    btn.attr("target", "FollowUser");
                    btn.removeClass("btn-secondary");
                    btn.addClass("btn-primary");
                    btn.html("Zaprati");
                }
            }
            else {
                alert(data);
            }
        }
    }); 
}



function loadUserImages() {

    var url = "/post/GetPostsByUser?user_name=" + userName;
    if (pagination != null) {
        url += "&pagination=" + pagination;
    }
    console.log("aaaa");
    $.ajax({
        type: "POST",
        url: url,
        processData: false,
        contentType: false,
        success: function (data) {
            var result = JSON.parse(data);
            var images = result.posts;
            if (result.pagination == "end") {
                pagination = null;
                $("#images-loader").html("");
            }
            else {
                pagination = result.pagination;
                $("#images-loader").html('<button id="load-more-images" class="btn btn-primary" type="button" onclick="loadUserImages()">Učitaj još</button>');
            }
                

            for (var i = 0; i < images.length; i++) {
                var string = '<div class="col-xs-4 image-thumbnail" post_id="' + images[i].post_id + '">'
                    + '<a href="/post/GetSinglePost?user=' + userName + '&id=' + images[i].post_id+'">'
                    + '<img src = "' + images[i].post_image + '" style="width:100%;" />'
                    + '</a></div >';
                $("#user_images").append(string);
            }
        }
    }); 
}

function showFollowers(element) {
    var btn = $(element);
    var target = $(element).attr("modal-target");
    var modal = $("#followers-modal ");
    $.ajax({
        type: "POST",
        url: "/users/" + target + "/?user_name=" + userName,
        processData: false,
        contentType: false,
        success: function (data) {
            modal.find(".modal-body").html(data);
            //modal.find(".modal-title").html(btn.attr("modal-title"));
            modal.modal();
        }
    }); 
}
