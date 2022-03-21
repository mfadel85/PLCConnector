<?php
phpinfo();
$test = [
    [0, 0, 1, 0, 0, 0, 1, 1, 0, 0],
    [0, 0, 1, 0, 0, 1, 1, 1, 0, 0],
    [0, 0, 0, 0, 0, 0, 1, 1, 1, 1],
    [0, 0, 0, 0, 0, 1, 1, 1, 1, 0],
];
$count = 3;
function getAvailableCells(&$unit, $beltCount)
{
    $line  = 0;
$modifiedUnit = array();
    foreach ($unit as $shelf) {
        $currentLine = $shelf;
        $line++;
        print_r("<BR> NEW LINE:Line $line <BR>");
        for($k=0;$k<8;$k++){
            $counter = 0;
            print_r("<BR>");
            for ($j = $k; $j < $k + 3; $j++) {
                print_r(" K is $k J is : $j<BR>");
                if ($shelf[$j] == 0) {
                    print_r("<BR>Line: line $line 0Cell : $j  $k <BR>");
                    $counter++;
                    $index = $j;
                }
                else {
                    print_r("stat again <br>");
                    $counter = 0;
                    continue;

                }
            }
            if ($counter == $beltCount) {
                print_r("<BR>Availables: Line $line  K is $k <BR>");
                for($i=0;$i<$beltCount;$i++){
                    print_r(" changes :D ");
                    $currentLine[$k+$i] = -1;
                }
                
                
                $counter = 0;
            }
        }
        $modifiedUnit[] = $currentLine;

    }
    return $modifiedUnit;
}
$cells = getAvailableCells($test,$count);
print_r($cells);
