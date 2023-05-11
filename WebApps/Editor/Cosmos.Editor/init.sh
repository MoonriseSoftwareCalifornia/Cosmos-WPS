# start SSH
sed -i "s/SSH_PORT/$SSH_PORT/g" /etc/ssh/sshd_config
/usr/sbin/sshd


lsb_release -a

dotnet Cosmos.Cms.dll
