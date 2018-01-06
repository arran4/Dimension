<?PHP

$max_lines = 100; // Maximum number of entries 
$file = 'list.txt';

// Grab list file, split in to an array
$old_contents = file_get_contents (  $file );
$old_list = explode ( "\n", $old_contents ); 


// Only write to file if we are given a port number
if ( $_GET['port'] != '') {
	
	$total = count ( $old_list ); // Number of addresses stored

	// There is a maximum number of lines set and we've reached it, so we need to trim.
	if ($max_lines > 0 && $total >= $max_lines ) { 
		array_splice ( $old_list, $max_lines - 1 ); // Remove last element
	}


	$new_line[] = $_SERVER['REMOTE_ADDR'] . ' ' . $_GET['port'] ;

	// Insert new line at the start of the list
	$new_list = array_merge ( $new_line, $old_list );

		
	// Write file. 	
	$pointer = fopen ( $file, 'w');
	foreach ( $new_list as $line ) {
		
		// Clear whitespace, then check to make sure that there's still content - we dont' want a blank line!
		$line = trim ($line);
		if ( $line != '') {
			fwrite ( $pointer, $line . "\n");
			echo "{$line}\n";
		}
	}
	fclose ( $pointer);

}
else		// Just output the current file
{

echo $old_contents;

}
?>