$(document).ready(function () {
    $("#like-button").on("click", function () {
        likeUnlikePost(this);
    })

    $("#see-post-likes").on("click", function () {
        showPostLikes(this);
    })
})

function likeUnlikePost(element) {
    var btn = $(element);

    if (btn.attr("logged-in") == undefined) {
        alert("Morate biti ulogovani da bi lajkovali.");
        return;
    }

    var target = btn.attr("target");
    var post_id = btn.attr("post-id");

    $.ajax({
        type: "GET",
        url: "/post/" + target + "/?post_id=" + post_id,
        processData: false,
        contentType: false,
        success: function (data) {
            if (data == "success") {
                var likes = parseInt($(".post-single-likes-date strong").html());
                if (target == "LikePost") {
                    btn.attr("target", "UnlikePost");
                    btn.find("i").removeClass("far");
                    btn.find("i").addClass("fas");
                    $("#post-likes-" + post_id).html(likes + 1);
                }
                else {
                    btn.attr("target", "LikePost");
                    btn.find("i").removeClass("fas");
                    btn.find("i").addClass("far");
                    $("#post-likes-" + post_id).html(likes - 1);
                }
            }
            else {
                alert(data);
            }
        }
    }); 

}

function showPostLikes(element) {
    var btn = $(element);
    var post_id = $(element).attr("post_id");
    var modal = $("#followers-modal");
    $.ajax({
        type: "GET",
        url: "/post/GetUsersThatLikedPost/?post_id=" + post_id,
        processData: false,
        contentType: false,
        success: function (data) {
            modal.find(".modal-body").html(data);
            //modal.find(".modal-title").html(btn.attr("modal-title"));
            modal.modal();
        }
    }); 
}

function deletePost(element) {
    var btn = $(element);
    var post_id = $(element).attr("post_id");
    var userName = $(element).attr("user_name");
    $.ajax({
        type: "GET",
        url: "/post/DeletePost/?post_id=" + post_id,
        processData: false,
        contentType: false,
        success: function (data) {
            if (data == "success") {
                window.location.href = "/user/" + userName;
            }
            else {
                alert(data)
            }
        }
    }); 
}