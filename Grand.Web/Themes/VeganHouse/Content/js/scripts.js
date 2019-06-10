new WOW().init({ animateClass: "animated" });

$(document).ready(function () {

    /** Automatic bootstrap carousel */
    $('.carousel-auto').each(function () {
        var container = $(this);
        var images = container.find('img');
        var id = 'autocarousel';

        var newHtml = '<div class="carousel slide" data-ride="carousel" id="' + id + '">' +
                      '    <ol class="carousel-indicators">';

        for (var i = 0; i < images.length; i++) {
            newHtml += '        <li data-target="#' + id + '" data-slide-to="' + i + '" class="' + (i == 0 ? 'active' : '') + '"></li>';
        }

        newHtml += '    </ol>' +
                   '    <div class="carousel-inner">';

        for (var i = 0; i < images.length; i++) {
            var image = $(images[i]);
            newHtml += '        <div class="carousel-item ' + (i == 0 ? 'active' : '') + '">' +
                       '            <img class="d-block w-100 img-fluid" src="' + image.attr('src') + '" alt="' + image.attr('alt') + '">' +
                       '            <div class="carousel-caption d-none d-lg-block w-100 m-0 p-0">' +
                       '                <div class="row w-100"><div class="col-12 col-xl-2 carousel-caption-left"></div><div class="col-12 col-xl-7 carousel-caption-text"><h5>' + image.attr('title') + '</h5></div><div class="d-none d-xl-block col-xl-3"></div></div>' +
                       '            </div>' +
                       '        </div>';
        }

        newHtml += '    </div>' +
                   '    <a class="carousel-control-prev" href="#' + id + '" role="button" data-slide="prev">' +
                   '        <span class="carousel-control-prev-icon" aria-hidden="true"></span>' +
                   '        <span class="sr-only">Previous</span>' +
                   '    </a>' +
                   '    <a class="carousel-control-next" href="#' + id + '" role="button" data-slide="next">' +
                   '        <span class="carousel-control-next-icon" aria-hidden="true"></span>' +
                   '        <span class="sr-only">Next</span>' +
                   '    </a>' +
                   '</div>';

        container.html(newHtml);
    });

});