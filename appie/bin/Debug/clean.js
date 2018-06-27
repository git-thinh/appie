
var url = '[_URL_]';
var au = url.split('/');
var domain = au[0] + '://' + au[2];

var dels = [], a = [];

var h1s = document.getElementsByTagName('h1');
if (h1s.length == 0) h1s = document.getElementsByTagName('h2');

if (h1s.length > 0) {
    var it = h1s[h1s.length - 1].parentNode;
    while (it.parentNode.tagName != 'BODY') {
        it = it.parentNode;
    }

    dels = [], a = [];
    a = document.body.childNodes;
    for (var i = 0; i < a.length; i++)
        if (a[i] != it) dels.push(a[i]);
    for (var i = 0; i < dels.length; i++)
        if (dels[i] != null && dels[i].parentNode != null) dels[i].parentNode.removeChild(dels[i]);
}

a = document.body.getElementsByTagName('*');
for (var i = 0; i < a.length; i++) {
    if (a[i].hasAttribute('style')) a[i].removeAttribute('style');
    if (a[i].tagName == 'PRE') continue;
    if (a[i].tagName == 'CODE') {
        var code = a[i].innerHTML;
        code = code.split('<').join('&lt;').split('>').join('&gt;');
        a[i].innerHTML = code;
        //console.log(code);
    } else {
        if (is_remove_text(a[i].textContent) || is_remove_id(a[i].id) || is_remove_css(a[i].className)) dels.push(a[i]);
    }
}
for (var i = 0; i < dels.length; i++)
    if (dels[i] != null && dels[i].parentNode != null) dels[i].parentNode.removeChild(dels[i]);

function is_remove_text(s) {
    if (s == null || s.trim().length == 0) return true;
    s = s.toLowerCase().trim();
    return s == 'read more'
        || s == 'see more'
        || s == 'further reading'
        || s == 'log in'
        || s == 'ad'
        || s.indexOf('sign in') == 0
        || s.indexOf('sign up') == 0
        || s.indexOf('copyright ') == 0
}

function is_remove_id(s) {
    if (s == null || s.trim().length == 0) return false;
    s = s.toLowerCase();
    return s.indexOf('search') != -1
        //|| s.indexOf('header') != -1
        //|| s.indexOf('footer') != -1
        //|| s.indexOf('sidebar') != -1
        || s.indexOf('login') != -1
        || s.indexOf('cookie') != -1
        || s.indexOf('newsletter') != -1
        || s.indexOf('breadcrumb') != -1
        || s.indexOf('metadata') != -1
        || s.indexOf('feedback') != -1
        || s.indexOf('user-info') != -1
        || s.indexOf('post-menu') != -1
        || s.indexOf('share-') != -1
        || s.indexOf('share-') != -1
        || s.indexOf('nav-') != -1
        || s.indexOf('adbanner') != -1
        || s.indexOf('error') != -1
        || s.indexOf('navigation-') != -1
        || s.indexOf('page-action') == 0
        || s.indexOf('pageaction') == 0
        || s.indexOf('memberbar') != -1
        || s == 'sidebar'

        || s.indexOf('comments-link-') == 0
}

function is_remove_css(s) {
    if (s == null || s.trim().length == 0) return false;
    s = s.toLowerCase();
    return s.indexOf('hide') != -1
        || s.indexOf('search') != -1
        //|| s.indexOf('header') != -1
        //|| s.indexOf('footer') != -1
        //|| s.indexOf('sidebar') != -1
        || s.indexOf('cookie') != -1
        || s.indexOf('newsletter') != -1
        || s.indexOf('breadcrumb') != -1
        || s.indexOf('metadata') != -1
        || s.indexOf('feedback') != -1
        || s.indexOf('user-info') != -1
        || s.indexOf('post-menu') != -1
        || s.indexOf('share-') != -1
        || s.indexOf('share-') != -1
        || s.indexOf('nav-') != -1
        || s.indexOf('navbar') != -1
        || s.indexOf('adbanner') != -1
        || s.indexOf('error') != -1

        || s.indexOf('page-action') == 0
        || s.indexOf('pageaction') == 0
        || s.indexOf('memberbar') != -1

        || s.indexOf('rating') != -1
        || s.indexOf('social') != -1

        /*stackoverflow.com*/
        || s.indexOf('answers-subheader') != -1
        //|| s == 'vote'
        || s == 'vote-up-off'
        || s == 'vote-down-off'
        || s == 'bottom-notice'
        || s == 'question-status'
        || s == 'post-taglist'
        || s == 'comment-score'
        || s.indexOf('comment-user') != -1 
        || s.indexOf('vote-accepted') != -1 

        /*github.com*/
        || s.indexOf('signup-') != -1 
}  