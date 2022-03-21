<?php

define('VERSION', '3.0.3.2');

// Configuration
if (is_file('config.php')) {
    require_once 'config.php';
}

// Install
if (!defined('DIR_APPLICATION')) {
    header('Location: install/index.php');
    exit;
}

// Startup
require_once DIR_SYSTEM . 'startup.php';

start('catalog');


/*ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);
// Version
// Turn off all error reporting
error_reporting(0);

define('VERSION', '3.0.3.2');
// Configuration
if (is_file('config.php')) {
	require_once('config.php');
}

// Install
if (!defined('DIR_APPLICATION')) {
	header('Location: install/index.php');
	exit;
}

// VirtualQMOD
require_once('./vqmod/vqmod.php');
VQMod::bootup();

// VQMODDED Startup
require_once(VQMod::modCheck(DIR_SYSTEM . 'startup.php'));

start('catalog');*/

