var url = '[_URL_]';
var au = url.split('/');
var domain = au[0] + '://' + au[2];

var links = document.getElementsByTagName('a'), e_remove = [];
if (links.length > 0) {
    var a, href, text, remove = false;
    for (var i = 0; i < links.length; i++) {
        a = links[i], text = a.textContent, remove = false;
        if (text == null || text.trim().length == 0 || is_link_remove(text.trim().toLowerCase()) == true) remove = true;
        if (a.hasAttribute('href') == false) continue;

        if (remove) {
            e_remove.push(a);
        }
        else {
            href = a.getAttribute('href').trim();
            switch (href[0]) {
                case '#':
                    e_remove.push(a);
                    href = '';
                    break;
                case 'h':
                    if (href.indexOf('http') != 0) {
                        if (url[url.length - 1] == '/')
                            href = url + href;
                        else
                            href = url.substring(0, url.length - au[au.length - 1].length) + href;
                    }
                    break;
                case '/':
                    href = domain + href;
                    break;
                case '.':
                    e_remove.push(a);
                    break;
                default:
                    if (url[url.length - 1] == '/')
                        href = url + href;
                    else
                        href = url.substring(0, url.length - au[au.length - 1].length) + href;
                    break;
            }
            if (href.length > 0) {
                if (a.hasAttribute('target')) a.removeAttribute('target');
                if (a.hasAttribute('class')) a.removeAttribute('class');
                a.setAttribute('onclick', 'alert(\'' + href + '\')');
                a.setAttribute('href', 'javascript:void(-1);');
            }
        }
    }
}

if (e_remove.length > 0)
    for (var i = 0; i < e_remove.length; i++)
        e_remove[i].parentNode.removeChild(e_remove[i]);

e_remove = [];
var lis = document.getElementsByTagName('li');
if (lis.length > 0)
    for (var i = 0; i < lis.length; i++)
        if (lis[i].textContent == null || lis[i].textContent.trim().length == 0) e_remove.push(lis[i]);

if (e_remove.length > 0)
    for (var i = 0; i < e_remove.length; i++) e_remove[i].parentNode.removeChild(e_remove[i]);

var htm;
findMain(document.body.childNodes, 0);
//console.log('OK: ', htm == null ? 'NULL -> find H1' : htm.length);
if (htm == null) findH1();
if (htm == null) {
    //console.log('FAILLL: ');
    cleanHtml(document.getElementsByTagName('*'));
    clear_Style_Submit();
} else {
    document.body.innerHTML = htm;
    setTimeout(clear_Style_Submit, 100);
}

function clear_Style_Submit() {
    var eall = document.getElementsByTagName('*');
    for (var i = 0; i < eall.length; i++)
        if (eall[i].hasAttribute('style')) eall[i].removeAttribute('style');
}

function is_link_remove(s) {
    return s.indexOf('log in') != -1
        || s.indexOf('login') != -1
        || s.indexOf('read more') != -1;
}

function findH1() {
    var h1s = document.getElementsByTagName('h1');
    if (h1s.length > 0) {
        var it = h1s[h1s.length - 1].parentNode;
        while (it.parentNode.tagName != 'BODY') {
            it = it.parentNode;
        }
        var bs = document.getElementsByClassName(it.className);
        if (bs.length > 1) {
            for (var i = 0; i < bs.length; i++) {
                cleanHtml(bs[i].getElementsByTagName('*'));
                if (htm == null)
                    htm = bs[i].innerHTML;
                else
                    htm += bs[i].innerHTML;
            }
        } else {
            //console.log('OK = ' + h1s.length, it);
            cleanHtml(it.getElementsByTagName('*'));
            htm = it.innerHTML;
        }
    }
}

function findMain(elements) {
    for (var i = 0; i < elements.length; i++) {
        var it = elements[i], css = it.className, id = it.id, tagName = it.tagName, text = it.textContent, ok = false;

        if (it.hasChildNodes() == false) {
            //console.log('REMOVE: ', it);
            it.parentNode.removeChild(it);
        } else {
            if (css == null) css = ''; else css = css.toLowerCase();
            if (id == null) id = ''; else id = id.toLowerCase();
            if (text == null) text = ''; else text = text.trim();
            if (id.length == 0 && css.length == 0 || text.length == 0) continue;

            ok = is_content(id, css, tagName, text);

            if (ok) {
                var h1 = it.getElementsByTagName('h1').length;
                if (htm == null && h1 > 0) {
                    //console.log('OK = ' + h1, it);
                    cleanHtml(it.getElementsByTagName('*'));
                    htm = it.innerHTML;
                }
                //return;
            } else {
                //console.log(id + ': ' + css + ' remove = ' + is_remove(id, css), htm);
                if (htm != null || is_remove(id, css)) {
                    //console.log('remove: ', it);
                    it.parentNode.removeChild(it);
                } else
                    findMain(it.childNodes);
            }
        }
    }
}

function is_content(id, css, tagName, text) {
    var ok = (id.length > 0 && id.indexOf('main') != -1) || (css.length > 0 && css.indexOf('main') != -1)
        || (id.length > 0 && id.indexOf('content') != -1) || (css.length > 0 && css.indexOf('content') != -1);
    if (ok) {
        if (
            id.indexOf('menu') != -1
            || css.indexOf('menu') != -1
            || id.indexOf('search') != -1
            || css.indexOf('search') != -1
            || id.indexOf('header') != -1
            || css.indexOf('header') != -1
            || id.indexOf('footer') != -1
            || css.indexOf('footer') != -1
            || id.indexOf('adbanner') != -1
            || css.indexOf('adbanner') != -1
            || id.indexOf('cookie') != -1
            || css.indexOf('cookie') != -1
            || id.indexOf('feedback') != -1
            || css.indexOf('feedback') != -1
            || id.indexOf('error') != -1
            || css.indexOf('error') != -1
        ) return false;
        return true;
    }
    return false;
}

function cleanHtml(elements) {
    for (var i = 0; i < elements.length; i++) {
        var it = elements[i], css = it.className, id = it.id, tagName = it.tagName, text = it.textContent, ok = false;
        if (is_remove(id, css)) it.parentNode.removeChild(it);
    }
}

function is_remove(id, css) {
    return id.indexOf('search') != -1
        || css.indexOf('search') != -1
        || id.indexOf('header') != -1
        || css.indexOf('header') != -1
        || id.indexOf('footer') != -1
        || css.indexOf('footer') != -1
        || id.indexOf('cookie') != -1
        || css.indexOf('cookie') != -1
        || id.indexOf('newsletter') != -1
        || css.indexOf('newsletter') != -1
        || id.indexOf('breadcrumb') != -1
        || css.indexOf('breadcrumb') != -1
        || id.indexOf('metadata') != -1
        || css.indexOf('metadata') != -1
        || id.indexOf('feedback') != -1
        || css.indexOf('feedback') != -1
        || id.indexOf('sidebar') != -1
        || css.indexOf('sidebar') != -1
        || id.indexOf('user-info') != -1
        || css.indexOf('user-info') != -1
        || id.indexOf('post-menu') != -1
        || css.indexOf('post-menu') != -1
        //|| id.indexOf('page-background') != -1
        //|| css.indexOf('page-background') != -1
        || id.indexOf('share-') != -1
        || css.indexOf('share-') != -1
        || id.indexOf('share-') != -1
        || css.indexOf('-action') != -1
        || id.indexOf('nav-') != -1
        || css.indexOf('nav-') != -1
        || id.indexOf('adbanner') != -1
        || css.indexOf('adbanner') != -1
        || id.indexOf('error') != -1
        || css.indexOf('error') != -1;
}