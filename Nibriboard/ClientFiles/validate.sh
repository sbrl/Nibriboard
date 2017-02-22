#!/usr/bin/env bash

find . -name "*.js" -not -path "./node_modules/*" | while read filename;
do
	validate_result=$(node_modules/.bin/acorn --module --silent $filename 2>&1);
	validate_exit_code=$?;
	validate_output=$([[ ${validate_exit_code} -eq 0 ]] && echo ok || echo ${validate_result});
	echo ${filename}: ${validate_output}
	# TODO: Use /dev/shm here since apparently while is in a subshell, so it can't modify variables in the main program O.o
	if ! [ ${validate_exit_code} -eq 0 ]; then
		echo incrementing ${error_count} \($(expr ${error_count} + 1)\)
		error_count=$(expr ${error_count} + 1);
	fi
done

echo 
echo Errors: $error_count
