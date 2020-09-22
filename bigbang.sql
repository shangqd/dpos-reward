/*
Navicat MySQL Data Transfer

Source Server         : 台式机
Source Server Version : 50731
Source Host           : 192.168.0.113:3306
Source Database       : bigbang

Target Server Type    : MYSQL
Target Server Version : 50731
File Encoding         : 65001

Date: 2020-09-22 16:54:22
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for Block
-- ----------------------------
DROP TABLE IF EXISTS `Block`;
CREATE TABLE `block` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `hash` varchar(64) DEFAULT NULL,
  `fork_hash` varchar(64) DEFAULT NULL,
  `prev_hash` varchar(64) DEFAULT NULL,
  `time` bigint(20) DEFAULT NULL,
  `height` int(11) DEFAULT NULL,
  `type` varchar(16) DEFAULT NULL,
  `reward_address` varchar(64) DEFAULT NULL COMMENT '出块奖励地址',
  `reward_money` decimal(20,10) DEFAULT NULL COMMENT '出块奖励金额',
  `is_useful` bit(1) DEFAULT b'1' COMMENT '是否有效，为最长链的区块',
  `bits` int(255) DEFAULT NULL COMMENT '难度',
  `reward_state` bit(1) DEFAULT b'0' COMMENT 'DPOS出块收益计算状态',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `hash` (`hash`) USING BTREE,
  KEY `height` (`height`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=438852 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Table structure for DposDailyReward
-- ----------------------------
DROP TABLE IF EXISTS `DposDailyReward`;
CREATE TABLE `dposdailyreward` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `dpos_addr` varchar(64) DEFAULT NULL COMMENT 'dpos节点地址',
  `client_addr` varchar(64) DEFAULT NULL COMMENT '投票人地址',
  `payment_date` date DEFAULT NULL COMMENT '收益日期',
  `payment_money` decimal(20,10) DEFAULT NULL COMMENT '当日总收益',
  `txid` varchar(64) DEFAULT NULL COMMENT '支付ID',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2228 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Table structure for DposPayment
-- ----------------------------
DROP TABLE IF EXISTS `DposPayment`;
CREATE TABLE `dpospayment` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `dpos_addr` varchar(64) DEFAULT NULL COMMENT 'dpos 地址',
  `client_addr` varchar(64) DEFAULT NULL COMMENT '投票者收款地址',
  `payment_date` date DEFAULT NULL COMMENT '投票者收益的日期',
  `payment_money` decimal(20,10) DEFAULT NULL COMMENT '投票者在这个日期内的收益',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=42345 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for DposRewardDetails
-- ----------------------------
DROP TABLE IF EXISTS `DposRewardDetails`;
CREATE TABLE `dposrewarddetails` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `dpos_addr` varchar(64) DEFAULT NULL COMMENT '节点地址',
  `client_addr` varchar(64) DEFAULT NULL COMMENT '投票人(第三方投票和节点自投)',
  `vote_amount` decimal(20,10) DEFAULT NULL COMMENT '投票金额',
  `reward_money` decimal(20,10) DEFAULT NULL COMMENT '投票收益',
  `reward_date` date DEFAULT NULL COMMENT '收益日期',
  `block_height` int(11) DEFAULT NULL COMMENT '区块高度',
  `reward_state` bit(1) DEFAULT b'0' COMMENT '汇总状态，1表示已计算汇总，0表未计算汇总',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=686152 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Table structure for DposState
-- ----------------------------
DROP TABLE IF EXISTS `DposState`;
CREATE TABLE `dposstate` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `dpos_addr` varchar(64) DEFAULT NULL COMMENT 'dpos 地址',
  `client_addr` varchar(64) DEFAULT NULL COMMENT '投票者地址',
  `audit_date` date DEFAULT NULL COMMENT '清算日期',
  `audit_money` decimal(20,10) DEFAULT NULL COMMENT '清算金额',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `dpos_addr` (`dpos_addr`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=48463 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

-- ----------------------------
-- Table structure for Pool
-- ----------------------------
DROP TABLE IF EXISTS `Pool`;
CREATE TABLE `pool` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `address` varchar(64) NOT NULL COMMENT '地址',
  `name` varchar(64) NOT NULL COMMENT '矿池/节点名称',
  `type` varchar(10) DEFAULT NULL COMMENT '类型(pow或dpos)',
  `key` varchar(128) DEFAULT NULL COMMENT '节点调用的APIKey',
  `fee` decimal(20,10) DEFAULT NULL COMMENT '节点投票手续费率',
  PRIMARY KEY (`id`,`address`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Table structure for PVInfo
-- ----------------------------
DROP TABLE IF EXISTS `PVInfo`;
CREATE TABLE `pvinfo` (
  `pv_id` int(11) NOT NULL AUTO_INCREMENT,
  `pv_ip` varchar(255) DEFAULT NULL,
  `pv_date` datetime DEFAULT NULL,
  `pv_page` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`pv_id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Table structure for Task
-- ----------------------------
DROP TABLE IF EXISTS `Task`;
CREATE TABLE `task` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `forkid` varchar(64) DEFAULT NULL,
  `block_hash` varchar(64) DEFAULT NULL,
  `is_ok` bit(1) DEFAULT b'0',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
-- Table structure for Tx
-- ----------------------------
DROP TABLE IF EXISTS `Tx`;
CREATE TABLE `tx` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `block_hash` varchar(64) DEFAULT NULL,
  `txid` varchar(64) DEFAULT NULL,
  `form` varchar(64) DEFAULT NULL,
  `to` varchar(64) DEFAULT NULL,
  `amount` decimal(20,10) DEFAULT NULL,
  `free` decimal(20,10) DEFAULT NULL,
  `type` varchar(16) DEFAULT NULL,
  `lock_until` int(11) DEFAULT NULL,
  `n` tinyint(6) DEFAULT NULL,
  `spend_txid` varchar(64) DEFAULT NULL,
  `data` varchar(4096) DEFAULT NULL,
  `dpos_in` varchar(64) DEFAULT NULL COMMENT '投票的dpos地址',
  `client_in` varchar(64) DEFAULT NULL COMMENT '投票的客户地址',
  `dpos_out` varchar(64) DEFAULT NULL COMMENT '赎回的dpos地址',
  `client_out` varchar(64) DEFAULT NULL COMMENT '赎回的客户地址',
  PRIMARY KEY (`id`) USING BTREE,
  KEY `block_id` (`block_hash`) USING BTREE,
  KEY `txid` (`txid`) USING BTREE,
  KEY `spend_txid` (`spend_txid`) USING BTREE,
  KEY `to` (`to`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2655075 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;
