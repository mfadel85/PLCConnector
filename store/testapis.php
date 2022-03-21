<?php
ini_set('display_errors', '1');
ini_set('display_startup_errors', '1');
error_reporting(E_ALL);

// set IP address and API access key
// Initialize cURL.
$ch = curl_init();

// Set the URL that you want to GET by using the CURLOPT_URL option.
curl_setopt($ch, CURLOPT_URL, 'https://ipgeolocation.abstractapi.com/v1/?api_key=6aeb3a1df84740ae97accefef5213a94');

// Set CURLOPT_RETURNTRANSFER so that the content is returned as a variable.
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

// Set CURLOPT_FOLLOWLOCATION to true to follow redirects.
curl_setopt($ch, CURLOPT_FOLLOWLOCATION, true);

// Execute the request.
$data = curl_exec($ch);

// Close the cURL handle.
curl_close($ch);

// Print the data out onto the page.
echo $data;

