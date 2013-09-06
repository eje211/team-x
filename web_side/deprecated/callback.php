<?php
session_start();
// IMPORTANT: prevent any caching.
header("Cache-Control: no-cache, must-revalidate"); // HTTP/1.1
header("Expires: Sat, 26 Jul 1997 05:00:00 GMT"); // Date in the past
header("Pragma: no-cache");
 
if(!isset($_SESSION['hash']))
	$_SESSION['hash'] = substr(str_shuffle(str_repeat('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',5)),0,12);

$dsn = 'mysql:host=localhost;dbname=unity';
$options = array(
    PDO::MYSQL_ATTR_INIT_COMMAND => 'SET NAMES utf8',
);

if (!isset($_REQUEST['type'])) exit();

$action_type = $_REQUEST['type'];
unset($_REQUEST['type']);

$dbh = new PDO($dsn, 'unity', 'unity', $options);

$sth = $dbh->prepare('SELECT hash FROM actions WHERE hash = :hash AND type = :type');
$sth->execute(array(':hash' => $_SESSION['hash'], ':type' => $action_type));
$result = $sth->fetchAll();
$count = count($result);

if ($count)
	$sth = $dbh->prepare("UPDATE actions SET action = :action, done = 0 WHERE hash = :hash AND type = :type");
else
	$sth = $dbh->prepare("INSERT INTO actions SET hash = :hash, type = :type, action = :action, done = 0");
$sth->execute(array(
	':hash' => $_SESSION['hash'],
	':type' => $action_type,
	':action' => http_build_query($_REQUEST)
	));

//print_r($dbh->errorInfo());
//print_r($_SESSION);
print "INSERT actions SET hash = " . $_SESSION['hash'] . ", type = $action_type, action = " . http_build_query($_REQUEST) . ", done = 0";
//print_r($_REQUEST);
//print_r($result);
