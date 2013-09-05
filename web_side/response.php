<?php
session_start();

// if (!isset($_SESSION['hash'])) exit('no session');
if (!isset($_REQUEST['type'])) exit('no type');

$dsn = 'mysql:host=localhost;dbname=unity';
$options = array(
	PDO::MYSQL_ATTR_INIT_COMMAND => 'SET NAMES utf8',
);
$dbh = new PDO($dsn, 'unity', 'unity', $options);

$sth = $dbh->prepare("SELECT * FROM unityresponse WHERE hash = :hash AND type = :type");
$sth->execute(array(':hash' => $_SESSION['hash'], ':type' => $_REQUEST['type']));

echo json_encode($sth->fetchAll());

