#!/usr/bin/env bash

counter_filename=$(mktemp -p /dev/shm/ -t bash.XXXXXXXXX.tmp);
echo 0 >${counter_filename};
find . -name "*.js" -not -path "./node_modules/*" | while read filename;
do
	validate_result=$(node_modules/.bin/acorn --module --silent $filename 2>&1);
	validate_exit_code=$?;
	validate_output=$([[ ${validate_exit_code} -eq 0 ]] && echo ok || echo ${validate_result});
	echo ${filename}: ${validate_output}
	# TODO: Use /dev/shm here since apparently while is in a subshell, so it can't modify variables in the main program O.o
	if ! [ ${validate_exit_code} -eq 0 ]; then
		error_count=$(cat ${counter_filename});
		echo incrementing ${error_count} \($(expr ${error_count} + 1)\);
		echo $(expr ${error_count} + 1) >${counter_filename};
	fi
done

error_count=$(cat ${counter_filename});
echo Errors: $error_count

if [[ ${error_count} -ne 0 ]]; then
	exit 1;
fi

exit 0;
