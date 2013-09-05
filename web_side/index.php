<?php
session_start();
// IMPORTANT: prevent any caching.

header("Cache-Control: no-cache, must-revalidate"); // HTTP/1.1
header("Expires: Sat, 26 Jul 1997 05:00:00 GMT"); // Date in the past
header("Pragma: no-cache");
require_once 'PHPTAL.php';

$dsn = 'mysql:host=localhost;dbname=unity';
$options = array(
	PDO::MYSQL_ATTR_INIT_COMMAND => 'SET NAMES utf8',
);
$dbh = new PDO($dsn, 'unity', 'unity', $options);


if (isset($_REQUEST['action']))
	if ($_REQUEST['action'] == 'drop_player') {
		header('Location:thanks.html');
		$sth = $dbh->prepare('DELETE FROM players WHERE hash = :hash');
		$sth->execute(array(':hash' => $_SESSION['hash']));
		$sth = $dbh->prepare('INSERT INTO todelete SET hash = :hash');
		$sth->execute(array(':hash' => $_SESSION['hash']));
		unset($_SESSION['hash']);
		exit();
	}
	
if (isset($_SESSION['hash'])) {
	$sth = $dbh->prepare('SELECT hash FROM players WHERE hash = :hash');
	$sth->execute(array(':hash' => $_SESSION['hash']));
	if (!count($sth->fetchAll())) unset($_SESSION['hash']);
}

if (!isset($_SESSION['hash'])) {
	$_SESSION['hash'] = substr(str_shuffle(str_repeat('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',5)),0,12);
	$sth = $dbh->prepare('INSERT INTO players SET hash = :hash');
	$sth->execute(array(':hash' => $_SESSION['hash']));
}

// print_r($_SESSION);

$data = new stdClass;
$data->ip  = $_SERVER['REMOTE_ADDR'];
$data->uas = $_SERVER['HTTP_USER_AGENT'];

$template = new PHPTAL('template.html');
$template->data = $data;
print $template->execute();
